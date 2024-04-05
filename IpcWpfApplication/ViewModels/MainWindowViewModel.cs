using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Ipc.Base;

namespace IpcWpfApplication.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IIpcMmfServer<string> _mmfServer;
        private readonly IIpcMmfClient<string> _mmfClient;

        private bool _flag = false;
        private bool _canLogin = true;
        private string _status;
        private string _actionText;
        public ICommand Login { get; }

        public string Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        public string ActionText
        {
            get => _actionText;
            set => SetField(ref _actionText, value);
        }

        public MainWindowViewModel()
        {
            Login = new RelayCommand(LoginHandler, CanLogin);
            ActionText = "Start Polling";

            _mmfServer = UnifiedMmfCommunicator.Instance;
            _mmfClient = UnifiedMmfCommunicator.Instance;

            var args = Environment.GetCommandLineArgs();
            if (args.Any(arg => arg.Contains("launchUrl")))
            {
                Status = "Sending data to other process";
                _mmfServer.Send(string.Join(Environment.NewLine, args));

                Application.Current.Shutdown();
            }
        }

        private void MmfClientDataReceived(string data)
        {
            Status = $"Data received from other process:{Environment.NewLine}{data}";
        }

        private bool CanLogin() => _canLogin;
        

        

        private void LoginHandler()
        {
            _canLogin = false;

            if (_flag == false)
            {
                ActionText = "Stop Polling";

                _mmfClient.DataReceived += MmfClientDataReceived;
                _mmfClient.StartPolling();

                Status = "Polling for other process message";

                _flag = true;
            }
            else
            {
                ActionText = "Start Polling";

                _mmfClient.DataReceived -= MmfClientDataReceived;
                _mmfClient.StopPolling();

                Status = "Stopped Polling for other process message!";

                _flag = false;
            }

            _canLogin = true;
        }
    }
}
