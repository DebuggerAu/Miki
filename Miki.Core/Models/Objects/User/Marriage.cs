﻿using IA;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace Miki.Models
{
    public class Marriage
    {
		public long MarriageId { get; set; }
		public List<UserMarriedTo> Participants { get; set; }
        public bool IsProposing { get; set; }
        public DateTime TimeOfMarriage { get; set; }
        public DateTime TimeOfProposal { get; set; }

		public ulong GetMe(ulong id) 
			=> GetMe(id.ToDbLong()).FromDbLong();
        public long GetMe(long id)
        {
			return Participants.FirstOrDefault(x => x.UserId == id).UserId;
        }

        public ulong GetOther(ulong id) 
			=> GetOther(id.ToDbLong()).FromDbLong();
        public long GetOther(long id)
        {
			return Participants.FirstOrDefault(x => x.UserId != id).UserId;
		}

		public void AcceptProposal(MikiContext context)
        {
            TimeOfMarriage = DateTime.Now;
            IsProposing = false;
        }

        public async Task RemoveAsync(MikiContext context)
        {
            context.Marriages.Remove(this);
            await context.SaveChangesAsync();
        }
		
        public static async Task DeclineAllProposalsAsync(MikiContext context, long me)
        {
            List<Marriage> proposals = await InternalGetProposalsReceivedAsync(context, me);
			proposals.AddRange(await InternalGetProposalsSentAsync(context, me));
            List<Marriage> disposableProposals = new List<Marriage>();
            context.Marriages.RemoveRange(proposals);
            await context.SaveChangesAsync();
        }

        public static async Task DivorceAllMarriagesAsync(MikiContext context, long me)
        {
            List<Marriage> marriages = await InternalGetMarriagesAsync(context, me);
            context.Marriages.RemoveRange(marriages);
            await context.SaveChangesAsync();
        }

        public static async Task<bool> ExistsAsync(MikiContext context, ulong id1, ulong id2) 
			=> await ExistsAsync(context, id1.ToDbLong(), id2.ToDbLong());
        public static async Task<bool> ExistsAsync(MikiContext context, long id1, long id2)
        {
            return await GetEntryAsync(context, id1, id2) != null;
        }

        public static async Task<bool> ExistsAsMarriageAsync(MikiContext context, long id1, long id2)
        {
            return await GetMarriageAsync(context, id1, id2) != null;
        }

        public static async Task<Marriage> GetProposalAsync(MikiContext context, ulong receiver, ulong asker)
        {
            return await GetProposalAsync(context, receiver.ToDbLong(), asker.ToDbLong());
        }
        public static async Task<Marriage> GetProposalAsync(MikiContext context, long receiver, long asker)
        {
            Marriage m = null;
            m = await InternalGetProposalAsync(context, receiver, asker);
            if (m == null) m = await InternalGetProposalAsync(context, asker, receiver);
            return m;
        }

        public static async Task<Marriage> GetProposalReceivedAsync(MikiContext context, ulong receiver, ulong asker)
        {
            return await GetProposalReceivedAsync(context, receiver.ToDbLong(), asker.ToDbLong());
        }
        public static async Task<Marriage> GetProposalReceivedAsync(MikiContext context, long receiver, long asker)
        {
            Marriage m = null;
            m = await InternalGetProposalAsync(context, receiver, asker);
            return m;
        }

        public static async Task<List<Marriage>> GetProposalsSent(MikiContext context, long asker) 
			=> await InternalGetProposalsSentAsync(context, asker);

        public static async Task<List<Marriage>> GetProposalsReceived(MikiContext context, long asker) 
			=> await InternalGetProposalsReceivedAsync(context, asker);

        public static async Task<Marriage> GetMarriageAsync(MikiContext context, ulong receiver, ulong asker) 
			=> await GetMarriageAsync(context, receiver.ToDbLong(), asker.ToDbLong());
        public static async Task<Marriage> GetMarriageAsync(MikiContext context, long receiver, long asker)
        {
            Marriage m = null;
            m = await InternalGetMarriageAsync(context, receiver, asker);
            if (m == null) m = await InternalGetMarriageAsync(context, asker, receiver);
            return m;
        }

        public static async Task<List<Marriage>> GetMarriagesAsync(MikiContext context, long userid)
        {
            return await InternalGetMarriagesAsync(context, userid);
        }

        public static async Task<Marriage> GetEntryAsync(MikiContext context, ulong receiver, ulong asker) 
			=> await GetEntryAsync(context, receiver.ToDbLong(), asker.ToDbLong());
        public static async Task<Marriage> GetEntryAsync(MikiContext context, long receiver, long asker)
        {
            Marriage m = null;
            m = await context.Marriages
				.Where(x => 
					x.Participants
						.Any(c => c.UserId == receiver) && 
					x.Participants
						.Any(c => c.UserId == asker))
				.FirstOrDefaultAsync();
            return m;
        }

        public static async Task<bool> IsBeingProposedBy(MikiContext context, long receiver, long asker)
        {
            return await InternalGetProposalAsync(context, receiver, asker) != null;
        }

        public static async Task<bool> ProposeAsync(MikiContext context, long receiver, long asker)
        {
            Marriage m = context.Marriages.Add(new Marriage()
            {
                IsProposing = true,
                TimeOfProposal = DateTime.Now,
                TimeOfMarriage = DateTime.Now,
            }).Entity;
			m.Participants = new List<UserMarriedTo>();
			m.Participants.Add(new UserMarriedTo()
			{
				MarriageId = m.MarriageId,
				UserId = receiver
			});
			m.Participants.Add(new UserMarriedTo()
			{
				MarriageId = m.MarriageId,
				UserId = asker,
				Asker = true
			});

			await context.SaveChangesAsync();
            return true;
        }

        private static async Task<Marriage> InternalGetProposalAsync(MikiContext context, long receiver, long asker)
        {
            return await context
                .Marriages
                .FindAsync(receiver, asker);
        }

        private static async Task<List<Marriage>> InternalGetProposalsSentAsync(MikiContext context, long asker)
        {
            return await context.Marriages
				.Where(p => p.Participants.FirstOrDefault(x => x.UserId == asker) != null && p.IsProposing == true)
				.ToListAsync();
        }

        private static async Task<List<Marriage>> InternalGetProposalsReceivedAsync(MikiContext context, long receiver)
        {
            return await context.Marriages
				.Where(p => p.Participants.FirstOrDefault(x => x.UserId == receiver) != null && p.IsProposing == true)
				.ToListAsync();
        }

        private static async Task<Marriage> InternalGetMarriageAsync(MikiContext context, long receiver, long asker)
        {
            return await context.Marriages
				.Where(tm => tm.Participants.FirstOrDefault(x => x.UserId == receiver) != null && tm.Participants.FirstOrDefault(x => x.UserId == receiver) != null && tm.IsProposing == false)
				.FirstOrDefaultAsync();
        }

        private async static Task<List<Marriage>> InternalGetMarriagesAsync(MikiContext context, long userid)
        {
            return await context
                .Marriages
                .Where(p => p.Participants.FirstOrDefault(x => x.UserId == userid) != null && !p.IsProposing)
				.ToListAsync();
        }
    }
}