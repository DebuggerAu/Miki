﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miki.API.Cards.Objects
{
    public class CardHand
    {
        public List<Card> Hand = new List<Card>();

        public void AddToHand(Card card)
        {
            Hand.Add(card);
        }

        public string Print()
        {
            string output = "";
            Hand.ForEach((x) => output += x.Print());
            return output;
        }
    }
}
