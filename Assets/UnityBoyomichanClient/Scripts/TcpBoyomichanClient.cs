﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UnityBoyomichanClient
{
    public class TcpBoyomichanClient : IBoyomichanClient
    {
        private readonly string _host;
        private readonly int _port;
        private readonly CancellationTokenSource _disposableCts;

        public TcpBoyomichanClient(string host, int port)
        {
            _host = host;
            _port = port;
            _disposableCts = new CancellationTokenSource();
        }

        /// <summary>
        ///  棒読みちゃん発声状態を問い合わせる
        /// </summary>
        /// <returns>true 発声中 / false 発声していない、または通信失敗</returns>
        public async UniTask<bool> CheckNowPlayingAsync(CancellationToken cancellationToken =
            default)
        {
            return await CheckAsync(ns =>
            {
                using (var bw = new BinaryWriter(ns))
                {
                    ns.ReadTimeout = 50;
                    //0x0120はGetNowPlaying（音声再生状態の取得）
                    bw.Write((short)0x0120);
                    bw.Flush();
                    var br = new BinaryReader(ns);
                    return br.ReadByte() > 0;
                }
            }, false, cancellationToken);
        }

        /// <summary>
        /// 棒読みちゃんに発声してもらう
        /// </summary>
        /// <param name="message">メッセージ本文</param>
        /// <param name="speed">読み上げ速度(-1で棒読みちゃん側の設定)</param>
        /// <param name="pitch">音程（-1で棒読みちゃん側の設定)</param>
        /// <param name="volume">音量(-1で棒読みちゃん側の設定)</param>
        /// <param name="voiceType">声質</param>
        /// <param name="token">CancellationToken</param>
        /// <returns></returns>
        public async UniTask<TaskId> TalkAsync(string message,
            int speed,
            int pitch,
            int volume,
            VoiceType voiceType,
            CancellationToken cancellationToken = default)
        {
            await SendAsync(ns =>
            {
                using (var bw = new BinaryWriter(ns))
                {
                    var encoded = Encoding.UTF8.GetBytes(message);
                    var lengh = encoded.Length;
                    bw.Write((short)0x0001); //コマンド（ 0:メッセージ読み上げ）
                    bw.Write((short)speed); //速度    （-1:棒読みちゃん画面上の設定）
                    bw.Write((short)pitch); //音程    （-1:棒読みちゃん画面上の設定）
                    bw.Write((short)volume); //音量    （-1:棒読みちゃん画面上の設定）
                    bw.Write((short)voiceType);
                    //声質 （ 0:棒読みちゃん画面上の設定、1:女性1、2:女性2、3:男性1、4:男性2、5:中性、6:ロボット、7:機械1、8:機械2、10001～:SAPI5）
                    bw.Write((byte)0); //文字列のbyte配列の文字コード(0:UTF-8, 1:Unicode, 2:Shift-JIS)
                    bw.Write((int)lengh); //文字列のbyte配列の長さ
                    bw.Write(encoded); //文字列のbyte配列
                }
            }, cancellationToken);

            return TaskId.InvalidId;
        }

        /// <summary>
        /// 読み上げ一時停止
        /// </summary>
        public UniTask PauseAsync(CancellationToken cancellationToken = default)
        {
            return SendAsync(ns =>
            {
                using (var bw = new BinaryWriter(ns))
                {
                    bw.Write((short)0x0010);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 読み上げ再開
        /// </summary>
        public UniTask ResumeAsync(CancellationToken cancellationToken = default)
        {
            return SendAsync(ns =>
            {
                using (var bw = new BinaryWriter(ns))
                {
                    bw.Write((short)0x0020);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 現在の行をスキップし次の行へ
        /// </summary>
        public UniTask SkipAsync(CancellationToken cancellationToken = default)
        {
            return SendAsync(ns =>
            {
                using (var bw = new BinaryWriter(ns))
                {
                    bw.Write((short)0x0030);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 残りタスクを全てキャンセル
        /// </summary>
        public UniTask ClearAsync(CancellationToken cancellationToken = default)
        {
            return SendAsync(ns =>
            {
                using (var bw = new BinaryWriter(ns))
                {
                    bw.Write((short)0x0040);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 一時停止状態の確認
        /// </summary>
        /// <returns>true 一時停止中 / false 一時停止していない、または通信失敗</returns>
        public UniTask<bool> CheckPauseAsync(CancellationToken cancellationToken)
        {
            return CheckAsync(ns =>
            {
                using (var bw = new BinaryWriter(ns))
                {
                    ns.ReadTimeout = 50;
                    bw.Write((short)0x0110);
                    bw.Flush();
                    var br = new BinaryReader(ns);
                    return br.ReadByte() > 0;
                }
            }, false, cancellationToken);
        }

        /// <summary>
        /// 残りタスク数取得
        /// </summary>
        /// <returns>残りのタスク数、通信失敗時は-1</returns>
        public UniTask<int> GetTaskCountAsync(CancellationToken cancellationToken)
        {
            return CheckAsync(ns =>
            {
                using (var bw = new BinaryWriter(ns))
                {
                    ns.ReadTimeout = 50;
                    bw.Write((short)0x0130);
                    bw.Flush();
                    var br = new BinaryReader(ns);
                    return br.ReadInt32();
                }
            }, -1, cancellationToken);
        }


        private async UniTask<T> CheckAsync<T>(Func<NetworkStream, T> func,
            T defaultValue,
            CancellationToken cancellationToken = default)
        {
            var ct = CancellationTokenSource.CreateLinkedTokenSource(_disposableCts.Token, cancellationToken).Token;
            try
            {
                return await Task.Run(() =>
                {
                    using (var tcpClient = new TcpClient())
                    {
                        tcpClient.SendTimeout = 1;
                        tcpClient.ReceiveTimeout = 1;
                        tcpClient.Connect(_host, _port);
                        ct.ThrowIfCancellationRequested();
                        using (var ns = tcpClient.GetStream())
                        {
                            return func(ns);
                        }
                    }
                }, ct);
            }
            catch (TaskCanceledException)
            {
                return defaultValue;
            }
            catch (OperationCanceledException)
            {
                return defaultValue;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return defaultValue;
            }
        }

        private async UniTask SendAsync(Action<NetworkStream> act, CancellationToken cancellationToken)
        {
            var ct = 
                CancellationTokenSource.CreateLinkedTokenSource(_disposableCts.Token, cancellationToken).Token;
            try
            {
                await Task.Run(() =>
                {
                    using (var tcpClient = new TcpClient())
                    {
                        tcpClient.SendTimeout = 1;
                        tcpClient.ReceiveTimeout = 1;
                        tcpClient.Connect(_host, _port);
                        ct.ThrowIfCancellationRequested();
                        using (var ns = tcpClient.GetStream())
                        {
                            act(ns);
                        }
                    }
                }, ct);
            }
            catch (TaskCanceledException)
            {
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Dispose()
        {
            _disposableCts.Cancel();
            _disposableCts.Dispose();
        }
    }
}