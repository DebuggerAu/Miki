﻿using IA;
using IA.SDK.Interfaces;
using Miki.Accounts.Achievements.Objects;
using Miki.Models;
using System;
using System.Threading.Tasks;

namespace Miki.Accounts.Achievements
{
    public class BaseAchievement
    {
        public string Name { get; set; } = Constants.NotDefined;
        public string ParentName { get; set; } = Constants.NotDefined;

        public string Icon { get; set; } = Constants.NotDefined;
		public int Points { get; set; } = 5;

        public BaseAchievement()
        {
        }

        public BaseAchievement(Action<BaseAchievement> act)
        {
            act.Invoke(this);
        }

        public virtual async Task<bool> CheckAsync(BasePacket packet)
        {
            await Task.Delay(0);
            return true;
        }

        /// <summary>
        /// Unlocks the achievement and if not yet added to the database, It'll add it to the database.
        /// </summary>
        /// <param name="context">sql context</param>
        /// <param name="id">user id</param>
        /// <param name="r">rank set to (optional)</param>
        /// <returns></returns>
        internal async Task UnlockAsync(IDiscordMessageChannel channel, IDiscordUser user, int r = 0)
        {
            long userid = user.Id.ToDbLong();

            Achievement a = null;

            using (var context = new MikiContext())
            {
                a = await context.Achievements.FindAsync(userid, ParentName);

                if (a != null || r != 0)
                {
                    if (a.Rank == r - 1)
                    {
                        a.Rank += 1;
                    }
                }
                else
                {
                    context.Achievements.Add(new Achievement() { Id = userid, Name = ParentName, Rank = 0 });
                }
                await context.SaveChangesAsync();
            }

            await Notification.SendAchievement(this, channel, user);
        }
    }
}