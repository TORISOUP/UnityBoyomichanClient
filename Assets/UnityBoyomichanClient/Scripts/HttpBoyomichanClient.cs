using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace UnityBoyomichanClient
{
    public class HttpBoyomichanClient : IBoyomichanClient
    {
        private readonly string _host;
        private readonly int _port;

        private string BaseUrl => $"http://{_host}:{_port}";

        public HttpBoyomichanClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async UniTask<bool> CheckNowPlayingAsync(CancellationToken cancellationToken = default)
        {
            using var uwr = UnityWebRequest.Get($"{BaseUrl}/getnowplaying");
            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

                // {"nowPlaying":[t]rue}
                return uwr.downloadHandler.text[15] == 't';
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
            throw new System.NotImplementedException();
        }

        public UniTask PauseAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public UniTask ResumeAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public UniTask SkipAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public UniTask ClearAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public UniTask<bool> CheckPauseAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public UniTask<int> GetTaskCountAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}