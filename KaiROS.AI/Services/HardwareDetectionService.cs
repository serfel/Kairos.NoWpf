using KaiROS.AI.Models;
using System.Management;
using System.Runtime.InteropServices;

namespace KaiROS.AI.Services;

public class HardwareDetectionService : IHardwareDetectionService
{
    private HardwareInfo? _cachedInfo;
    
    public async Task<HardwareInfo> DetectHardwareAsync()
    {
        if (_cachedInfo != null) return _cachedInfo;
        
        var info = new HardwareInfo();
        
        await Task.Run(() =>
        {
            // Detect RAM
            try
            {
                var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
                info.TotalRamBytes = (long)computerInfo.TotalPhysicalMemory;
                info.AvailableRamBytes = (long)computerInfo.AvailablePhysicalMemory;
            }
            catch
            {
                // Fallback: use GC info
                info.TotalRamBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
                info.AvailableRamBytes = info.TotalRamBytes - Environment.WorkingSet;
            }
            
            // Detect GPU via WMI
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                foreach (ManagementObject mo in searcher.Get())
                {
                    info.GpuName = mo["Name"]?.ToString();
                    if (mo["AdapterRAM"] is uint ram)
                        info.GpuMemoryBytes = ram;
                    break; // Use first GPU
                }
            }
            catch
            {
                info.GpuName = "Unknown";
            }
            
            // Check for CUDA availability (NVIDIA GPU)
            info.HasCuda = !string.IsNullOrEmpty(info.GpuName) && 
                           info.GpuName.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase);
            
            // DirectML is available on Windows 10+ with any GPU
            info.HasDirectML = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && 
                               Environment.OSVersion.Version.Major >= 10;
            
            // NPU detection (limited - check for Intel NPU or Qualcomm)
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%NPU%' OR Name LIKE '%Neural%'");
                info.HasNpu = searcher.Get().Count > 0;
            }
            catch
            {
                info.HasNpu = false;
            }
            
            // Build available backends list
            info.AvailableBackends.Add(ExecutionBackend.Cpu); // Always available
            
            if (info.HasCuda)
                info.AvailableBackends.Add(ExecutionBackend.Cuda);
            
            if (info.HasDirectML)
                info.AvailableBackends.Add(ExecutionBackend.DirectML);
            
            if (info.HasNpu)
                info.AvailableBackends.Add(ExecutionBackend.Npu);
            
            // Determine recommended backend
            info.RecommendedBackend = DetermineRecommendedBackend(info);
            info.SelectedBackend = info.RecommendedBackend;
            
            // Build status message
            info.StatusMessage = BuildStatusMessage(info);
        });
        
        _cachedInfo = info;
        return info;
    }
    
    public ExecutionBackend GetRecommendedBackend()
    {
        return _cachedInfo?.RecommendedBackend ?? ExecutionBackend.Cpu;
    }
    
    public bool IsBackendAvailable(ExecutionBackend backend)
    {
        return _cachedInfo?.AvailableBackends.Contains(backend) ?? backend == ExecutionBackend.Cpu;
    }
    
    private static ExecutionBackend DetermineRecommendedBackend(HardwareInfo info)
    {
        // Prefer CUDA for NVIDIA GPUs (best performance)
        if (info.HasCuda)
            return ExecutionBackend.Cuda;
        
        // NPU if available (power efficient)
        if (info.HasNpu)
            return ExecutionBackend.Npu;
        
        // DirectML for AMD/Intel GPUs on Windows
        if (info.HasDirectML && info.GpuMemoryBytes > 2L * 1024 * 1024 * 1024)
            return ExecutionBackend.DirectML;
        
        // Fallback to CPU
        return ExecutionBackend.Cpu;
    }
    
    private static string BuildStatusMessage(HardwareInfo info)
    {
        var parts = new List<string>
        {
            $"RAM: {info.TotalRamText}"
        };
        
        if (!string.IsNullOrEmpty(info.GpuName))
            parts.Add($"GPU: {info.GpuName}");
        
        if (info.HasCuda)
            parts.Add("CUDA ✓");
        else if (info.HasDirectML)
            parts.Add("DirectML ✓");
        
        if (info.HasNpu)
            parts.Add("NPU ✓");
        
        return string.Join(" | ", parts);
    }
}
