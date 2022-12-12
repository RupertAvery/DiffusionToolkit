using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Diffusion.Toolkit.Classes;

public class AsyncCommand : IAsyncCommand
{
    public event EventHandler CanExecuteChanged;

    private bool _isExecuting;
    private readonly Func<Task> _execute;
    private readonly Func<bool> _canExecute;
    private readonly IErrorHandler _errorHandler;

    public AsyncCommand(
        Func<Task> execute,
        Func<bool> canExecute = null,
        IErrorHandler errorHandler = null)
    {
        _execute = execute;
        _canExecute = canExecute;
        _errorHandler = errorHandler;
    }

    public bool CanExecute()
    {
        return !_isExecuting && (_canExecute?.Invoke() ?? true);
    }

    public async Task ExecuteAsync()
    {
        if (CanExecute())
        {
            try
            {
                _isExecuting = true;
                await _execute();
            }
            finally
            {
                _isExecuting = false;
            }
        }

        RaiseCanExecuteChanged();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    #region Explicit implementations
    bool ICommand.CanExecute(object parameter)
    {
        return CanExecute();
    }

    void ICommand.Execute(object parameter)
    {
        ExecuteAsync().FireAndForgetSafeAsync(_errorHandler);
    }
    #endregion
}