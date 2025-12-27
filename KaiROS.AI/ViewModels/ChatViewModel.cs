using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KaiROS.AI.Models;
using KaiROS.AI.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace KaiROS.AI.ViewModels;

public partial class ChatViewModel : ViewModelBase
{
    private readonly IChatService _chatService;
    private readonly IModelManagerService _modelManager;
    private CancellationTokenSource? _currentInferenceCts;
    
    [ObservableProperty]
    private ObservableCollection<ChatMessageViewModel> _messages = new();
    
    [ObservableProperty]
    private string _userInput = string.Empty;
    
    [ObservableProperty]
    private string _systemPrompt = "You are a helpful, friendly AI assistant. Be concise and clear.";
    
    [ObservableProperty]
    private bool _isGenerating;
    
    [ObservableProperty]
    private bool _isSystemPromptExpanded;
    
    [ObservableProperty]
    private double _tokensPerSecond;
    
    [ObservableProperty]
    private int _totalTokens;
    
    [ObservableProperty]
    private string _memoryUsage = "N/A";
    
    [ObservableProperty]
    private string _elapsedTime = "0s";
    
    [ObservableProperty]
    private bool _hasActiveModel;
    
    [ObservableProperty]
    private string _activeModelInfo = "No model loaded";
    
    public ChatViewModel(IChatService chatService, IModelManagerService modelManager)
    {
        _chatService = chatService;
        _modelManager = modelManager;
        
        _chatService.StatsUpdated += OnStatsUpdated;
        _modelManager.ModelLoaded += OnModelLoaded;
        _modelManager.ModelUnloaded += OnModelUnloaded;
    }
    
    private void OnModelLoaded(object? sender, LLMModelInfo model)
    {
        HasActiveModel = true;
        ActiveModelInfo = $"{model.DisplayName} ({model.SizeText})";
    }
    
    private void OnModelUnloaded(object? sender, EventArgs e)
    {
        HasActiveModel = false;
        ActiveModelInfo = "No model loaded";
    }
    
    private void OnStatsUpdated(object? sender, InferenceStats stats)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            TokensPerSecond = Math.Round(stats.TokensPerSecond, 1);
            TotalTokens = stats.TotalTokens;
            MemoryUsage = stats.MemoryUsageText;
            ElapsedTime = $"{stats.ElapsedTime.TotalSeconds:F1}s";
        });
    }
    
    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(UserInput) || IsGenerating)
            return;
        
        if (!_chatService.IsModelLoaded)
        {
            Messages.Add(new ChatMessageViewModel(ChatMessage.Assistant("Please load a model first from the Models tab.")));
            return;
        }
        
        // Add user message
        var userMessage = ChatMessage.User(UserInput);
        Messages.Add(new ChatMessageViewModel(userMessage));
        UserInput = string.Empty;
        
        // Prepare messages for inference
        var allMessages = new List<ChatMessage>();
        if (!string.IsNullOrWhiteSpace(SystemPrompt))
        {
            allMessages.Add(ChatMessage.System(SystemPrompt));
        }
        allMessages.AddRange(Messages.Select(m => m.Message));
        
        // Create assistant message for streaming
        var assistantMessage = ChatMessage.Assistant(string.Empty);
        assistantMessage.IsStreaming = true;
        var assistantVm = new ChatMessageViewModel(assistantMessage);
        Messages.Add(assistantVm);
        
        IsGenerating = true;
        _currentInferenceCts = new CancellationTokenSource();
        
        try
        {
            await foreach (var token in _chatService.GenerateResponseStreamAsync(allMessages, _currentInferenceCts.Token))
            {
                assistantVm.AppendContent(token);
            }
        }
        catch (OperationCanceledException)
        {
            assistantVm.AppendContent("\n[Generation stopped]");
        }
        catch (Exception ex)
        {
            assistantVm.Content = $"Error: {ex.Message}";
        }
        finally
        {
            assistantVm.Message.IsStreaming = false;
            assistantVm.IsStreaming = false;
            IsGenerating = false;
            _currentInferenceCts = null;
        }
    }
    
    [RelayCommand]
    private void StopGeneration()
    {
        _currentInferenceCts?.Cancel();
    }
    
    [RelayCommand]
    private void ClearChat()
    {
        Messages.Clear();
        _chatService.ClearContext();
        TokensPerSecond = 0;
        TotalTokens = 0;
        MemoryUsage = "N/A";
        ElapsedTime = "0s";
    }
    
    [RelayCommand]
    private async Task ExportChat()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json|Text files (*.txt)|*.txt",
            DefaultExt = "json",
            FileName = $"chat_export_{DateTime.Now:yyyyMMdd_HHmmss}"
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                if (dialog.FileName.EndsWith(".json"))
                {
                    var export = Messages.Select(m => new
                    {
                        Role = m.Message.Role.ToString(),
                        m.Content,
                        m.Message.Timestamp
                    });
                    var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(dialog.FileName, json);
                }
                else
                {
                    var lines = Messages.Select(m => $"[{m.Message.Role}] {m.Content}");
                    await File.WriteAllLinesAsync(dialog.FileName, lines);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Export failed: {ex.Message}";
            }
        }
    }
    
    [RelayCommand]
    private void ToggleSystemPrompt()
    {
        IsSystemPromptExpanded = !IsSystemPromptExpanded;
    }
}

public partial class ChatMessageViewModel : ObservableObject
{
    public ChatMessage Message { get; }
    
    [ObservableProperty]
    private string _content;
    
    [ObservableProperty]
    private bool _isStreaming;
    
    public bool IsUser => Message.Role == ChatRole.User;
    public bool IsAssistant => Message.Role == ChatRole.Assistant;
    public bool IsSystem => Message.Role == ChatRole.System;
    public string Timestamp => Message.Timestamp.ToString("HH:mm");
    
    public ChatMessageViewModel(ChatMessage message)
    {
        Message = message;
        _content = message.Content;
        _isStreaming = message.IsStreaming;
    }
    
    public void AppendContent(string text)
    {
        Content += text;
        Message.Content = Content;
    }
}
