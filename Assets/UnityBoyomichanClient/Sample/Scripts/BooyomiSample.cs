using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        [SerializeField] private Button _speechButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _clearButton;
        [SerializeField] private Button _skipButton;

        [SerializeField] private InputField _textInputField;

        [SerializeField] private Text _taskCounText;
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _playingText;

        [SerializeField] private string _host = "127.0.0.1";
        [SerializeField] private int _port = 50001;

        private CancellationTokenSource _cancellationTokenSource;
        private BoyomiClient _boyomiClient;

        private void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _boyomiClient = new BoyomiClient(_host, _port);

            _voiceTypeDropdown.options =
                Enum.GetNames(typeof(VoiceType)).Select(x => new Dropdown.OptionData(x)).ToList();

            ChechTaskCount(_cancellationTokenSource.Token).FireAndForget();
            ChechSpeeching(_cancellationTokenSource.Token).FireAndForget();
            ChechPause(_cancellationTokenSource.Token).FireAndForget();

            // Pause
            _pauseButton.onClick.AddListener(() =>
            {
                _boyomiClient.PauseAsync(_cancellationTokenSource.Token).FireAndForget();
            });

            // Resume
            _resumeButton.onClick.AddListener(() =>
            {
                _boyomiClient.ResumeAsync(_cancellationTokenSource.Token).FireAndForget();
            });

            // Skip
            _skipButton.onClick.AddListener(() =>
            {
                _boyomiClient.SkipAsync(_cancellationTokenSource.Token).FireAndForget();
            });

            // Clear
            _clearButton.onClick.AddListener(() =>
            {
                _boyomiClient.ClearAsync(_cancellationTokenSource.Token).FireAndForget();
            });

            // Speech
            _speechButton.onClick.AddListener(() =>
            {
                var text = _textInputField.text;
                if (text.Length == 0) return;

                var speed = _speedInputField.text.SafeParse();
                var pitch = _pitchInputField.text.SafeParse();
                var volume = _volumeInputField.text.SafeParse();
                var type = (VoiceType) _voiceTypeDropdown.value;
                _boyomiClient.TalkAsync(text, speed, pitch, volume, type, _cancellationTokenSource.Token)
                    .FireAndForget();
            });
        }


        private async Task ChechTaskCount(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // 残りのタスク数取得
                var count = await _boyomiClient.GetTaskCountAsync(token);
                _taskCounText.text = $"Task count: {count}";
                await Task.Delay(TimeSpan.FromMilliseconds(500), token);
            }
        }

        private async Task ChechSpeeching(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // 発声中か
                var isSpeeching = await _boyomiClient.CheckNowPlaying(token);
                _playingText.text = isSpeeching ? "Speeching" : "Idle";
                await Task.Delay(TimeSpan.FromMilliseconds(500), token);
            }
        }

        private async Task ChechPause(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Pause中か
                var isPause = await _boyomiClient.CheckPauseAsync(token);
                _statusText.text = isPause ? "Pause" : "Idle";
                await Task.Delay(TimeSpan.FromMilliseconds(500), token);
            }
        }


        private void OnDestroy()
        {
            _boyomiClient = null;
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

        public static async void FireAndForget(this Task task)
        {
            try
            {
                await task;
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}