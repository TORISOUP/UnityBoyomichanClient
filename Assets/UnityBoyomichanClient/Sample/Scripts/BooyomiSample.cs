using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UnityBoyomichanClient.Sample
{
    internal class BooyomiSample : MonoBehaviour
    {
        [SerializeField] private InputField _speedInputField;
        [SerializeField] private InputField _pitchInputField;
        [SerializeField] private InputField _volumeInputField;
        [SerializeField] private Dropdown _voiceTypeDropdown;
        [SerializeField] private Dropdown _clientTypeDropdown;
        [SerializeField] private InputField _hostInput;
        [SerializeField] private InputField _portInput;
        [SerializeField] private Button _speechButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _clearButton;
        [SerializeField] private Button _skipButton;

        [SerializeField] private Button _connectButton;


        [SerializeField] private InputField _textInputField;

        [SerializeField] private Text _taskCounText;
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _playingText;


        private CancellationTokenSource _cancellationTokenSource;
        private IBoyomichanClient _boyomichanClient;

        private void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _connectButton.OnClickAsAsyncEnumerable(destroyCancellationToken)
                .Subscribe(_ =>
                {
                    if (_boyomichanClient == null)
                    {
                        _boyomichanClient = _clientTypeDropdown.value == 0
                            ? new TcpBoyomichanClient(_hostInput.text, ToPortNum())
                            : new HttpBoyomichanClient(_hostInput.text, ToPortNum());

                        _connectButton.GetComponentInChildren<Text>().text = "Disconnect";
                    }
                    else
                    {
                        _boyomichanClient?.Dispose();
                        _boyomichanClient = null;
                        _connectButton.GetComponentInChildren<Text>().text = "Connect";
                    }
                });


            _voiceTypeDropdown.options =
                Enum.GetNames(typeof(VoiceType)).Select(x => new Dropdown.OptionData(x)).ToList();

            CheckTaskCount(_cancellationTokenSource.Token).Forget();
            CheckSpeeching(_cancellationTokenSource.Token).Forget();
            CheckPause(_cancellationTokenSource.Token).Forget();

            // Pause
            _pauseButton.onClick.AddListener(() =>
            {
                _boyomichanClient.PauseAsync(_cancellationTokenSource.Token).Forget();
            });

            // Resume
            _resumeButton.onClick.AddListener(() =>
            {
                _boyomichanClient.ResumeAsync(_cancellationTokenSource.Token).Forget();
            });

            // Skip
            _skipButton.onClick.AddListener(() =>
            {
                _boyomichanClient.SkipAsync(_cancellationTokenSource.Token).Forget();
            });

            // Clear
            _clearButton.onClick.AddListener(() =>
            {
                _boyomichanClient.ClearAsync(_cancellationTokenSource.Token).Forget();
            });

            // Speech
            _speechButton.onClick.AddListener(() =>
            {
                var text = _textInputField.text;
                if (text.Length == 0) return;

                var speed = _speedInputField.text.SafeParse();
                var pitch = _pitchInputField.text.SafeParse();
                var volume = _volumeInputField.text.SafeParse();
                var type = (VoiceType)_voiceTypeDropdown.value;
                _boyomichanClient.TalkAsync(text, speed, pitch, volume, type, _cancellationTokenSource.Token)
                    .Forget();
            });
        }


        private async UniTask CheckTaskCount(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_boyomichanClient != null)
                {
                    // 残りのタスク数取得
                    var count = await _boyomichanClient.GetTaskCountAsync(token);
                    _taskCounText.text = $"Task count: {count}";
                }

                await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: token);
            }
        }

        private async UniTask CheckSpeeching(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // 発声中か
                if (_boyomichanClient != null)
                {
                    var isSpeeching = await _boyomichanClient.CheckNowPlayingAsync(token);
                    _playingText.text = isSpeeching ? "Speeching" : "Idle";
                }

                await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: token);
            }
        }

        private async UniTask CheckPause(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Pause中か
                if (_boyomichanClient != null)
                {
                    var isPause = await _boyomichanClient.CheckPauseAsync(token);
                    _statusText.text = isPause ? "Pause" : "Idle";
                }

                await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: token);
            }
        }

        private int ToPortNum()
        {
            return int.TryParse(_portInput.text, out var port) ? port : 0;
        }

        private void OnDestroy()
        {
            _boyomichanClient?.Dispose();
            _boyomichanClient = null;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }

    internal static class BoyomiSampleExtensions
    {
        public static int SafeParse(this string text)
        {
            var result = -1;
            if (int.TryParse(text, out result))
            {
                if (result < 50 || result > 200)
                {
                    return -1;
                }

                return result;
            }

            return result;
        }
    }
}