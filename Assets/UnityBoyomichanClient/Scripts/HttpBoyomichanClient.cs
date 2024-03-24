using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityBoyomichanClient
{
    public class HttpBoyomichanClient : IBoyomichanClient
    {
        private readonly string _host;
        private readonly int _port;
        private string BaseUrl => $"http://{_host}:{_port}";
        private readonly CancellationTokenSource _disposableCts = new();

        public HttpBoyomichanClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async UniTask<bool> CheckNowPlayingAsync(CancellationToken cancellationToken = default)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_disposableCts.Token, cancellationToken).Token;

            using var uwr = UnityWebRequest.Get($"{BaseUrl}/getnowplaying");
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);

                // {"nowPlaying":[t]rue}
                return uwr.downloadHandler.text[14] == 't';
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        public async UniTask<TaskId> TalkAsync(string message,
            int speed,
            int pitch,
            int volume,
            VoiceType voiceType,
            CancellationToken cancellationToken = default)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_disposableCts.Token, cancellationToken).Token;

            using var uwr =
                UnityWebRequest.Get(
                    $"{BaseUrl}/talk?text={message}&voice={(int)voiceType}&speed={speed}&tone={pitch}&volume={volume}");
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
                var dto = JsonUtility.FromJson<TaskIdDto>(uwr.downloadHandler.text);
                return new TaskId(dto.taskId);
            }
            catch (OperationCanceledException)
            {
                return new TaskId(1);
            }
        }

        public async UniTask PauseAsync(CancellationToken cancellationToken = default)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_disposableCts.Token, cancellationToken).Token;

            using var uwr = UnityWebRequest.Get($"{BaseUrl}/pause");
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async UniTask ResumeAsync(CancellationToken cancellationToken = default)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_disposableCts.Token, cancellationToken).Token;
            using var uwr = UnityWebRequest.Get($"{BaseUrl}/resume");
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async UniTask SkipAsync(CancellationToken cancellationToken = default)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_disposableCts.Token, cancellationToken).Token;

            using var uwr = UnityWebRequest.Get($"{BaseUrl}/skip");
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async UniTask ClearAsync(CancellationToken cancellationToken = default)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_disposableCts.Token, cancellationToken).Token;

            using var uwr = UnityWebRequest.Get($"{BaseUrl}/clear");
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async UniTask<bool> CheckPauseAsync(CancellationToken cancellationToken)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_disposableCts.Token, cancellationToken).Token;

            using var uwr = UnityWebRequest.Get($"{BaseUrl}/getpause");
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);

                // {"pause":[t]rue}
                return uwr.downloadHandler.text[10] == 't';
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        public async UniTask<int> GetTaskCountAsync(CancellationToken cancellationToken)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_disposableCts.Token, cancellationToken).Token;

            using var uwr = UnityWebRequest.Get($"{BaseUrl}/getnowtaskid");
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
                var dto = JsonUtility.FromJson<NowTaskIdDto>(uwr.downloadHandler.text);
                return dto.nowTaskId;
            }
            catch (OperationCanceledException)
            {
                return -1;
            }
        }

        public void Dispose()
        {
            _disposableCts.Cancel();
            _disposableCts.Dispose();
        }

        internal struct TaskIdDto
        {
            public int taskId;
        }

        internal struct NowTaskIdDto
        {
            public int nowTaskId;
        }
    }
}