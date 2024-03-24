using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace UnityBoyomichanClient
{
    public interface IBoyomichanClient : IDisposable
    {
        /// <summary>
        ///  棒読みちゃん発声状態を問い合わせる
        /// </summary>
        /// <returns>true 発声中 / false 発声していない、または通信失敗</returns>
        public UniTask<bool> CheckNowPlayingAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 棒読みちゃんに発声してもらう
        /// </summary>
        /// <param name="message">メッセージ本文</param>
        /// <param name="speed">読み上げ速度(-1で棒読みちゃん側の設定)</param>
        /// <param name="pitch">音程（-1で棒読みちゃん側の設定)</param>
        /// <param name="volume">音量(-1で棒読みちゃん側の設定)</param>
        /// <param name="voiceType">声質</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns></returns>
        public UniTask<TaskId> TalkAsync(string message,
            int speed,
            int pitch,
            int volume,
            VoiceType voiceType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 読み上げ一時停止
        /// </summary>
        public UniTask PauseAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 読み上げ再開
        /// </summary>
        public UniTask ResumeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 現在の行をスキップし次の行へ
        /// </summary>
        public UniTask SkipAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 残りタスクを全てキャンセル
        /// </summary>
        public UniTask ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 一時停止状態の確認
        /// </summary>
        /// <returns>true 一時停止中 / false 一時停止していない、または通信失敗</returns>
        public UniTask<bool> CheckPauseAsync(CancellationToken cancellationToken);

        /// <summary>
        /// 残りタスク数取得
        /// </summary>
        /// <returns>残りのタスク数、通信失敗時は-1</returns>
        public UniTask<int> GetTaskCountAsync(CancellationToken cancellationToken);
    }

    public enum VoiceType
    {
        DefaultVoice = 0,
        Female1,
        Female2,
        Male1,
        Male2,
        Neuter,
        Robot,
        Machine1,
        Machine2
    }

    public readonly struct TaskId
    {
        public int Id { get; }
        public bool IsValidId => Id > 0;

        public static readonly TaskId InvalidId = new TaskId(-1);

        public TaskId(int id)
        {
            Id = id;
        }
    }
}