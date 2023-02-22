﻿using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using ChainFx.Nodal;

namespace ChainBuy
{
    public class NoticeBot
    {
        static readonly ConcurrentDictionary<int, Notice> notices = new ConcurrentDictionary<int, Notice>();


        // the message sending thread
        static readonly Thread pusher = new Thread(PushCycle);


        /// <summary>
        /// To push / send notices to recipients.
        /// </summary>
        /// <param name="state"></param>
        static async void PushCycle(object state)
        {
            while (true)
            {
                Thread.Sleep(60000 * 7);

                // use same builder for each and every sent notice
                var sb = new StringBuilder();
                var nowStr = DateTime.Now.ToString("yyyy-MM-dd HH mm");
                foreach (var ety in notices)
                {
                    var ntc = ety.Value;
                    if (ntc.HasToPush)
                    {
                        ntc.PushToBuffer(sb);

                        // send
                        await WeixinUtility.SendNotifSmsAsync(ntc.Tel,
                            ntc.Name,
                            nowStr,
                            sb.ToString()
                        );
                    }

                    // reset buffer
                    sb.Clear();
                }
            }
        }

        public static void Start()
        {
            pusher.Start();
        }

        public static void Put(int noticeId, short slot, int num, decimal amt)
        {
            // get or create the notice
            var ntc = notices.GetOrAdd(noticeId, k =>
            {
                var org = Nodality.GrabObject<int, Org>(noticeId);

                return new Notice(noticeId, org.name, org.tel);
            });

            // put to slot
            ntc.Put(slot, num, amt);
        }

        public static int CheckPully(int noticeId, short slot)
        {
            if (notices.TryGetValue(noticeId, out var ntc))
            {
                return ntc.CheckPully(slot);
            }
            return 0;
        }

        public static int CheckAndClearPully(int noticeId, short slot)
        {
            if (notices.TryGetValue(noticeId, out var ntc))
            {
                return ntc.CheckAndClearPully(slot);
            }
            return 0;
        }
    }
}