// Auto-generated file. Do not edit.
using System;
using System.Collections.Generic;

namespace Watermelon
{
    public static class RewardsMap
    {
        public static Dictionary<Type, Type> ViewMap { get; } = GetMap();

        public static Dictionary<Type, Type> GetMap()
        {
            Dictionary<Type, Type> map = new Dictionary<Type, Type>();
            map[typeof(Watermelon.NoAdsReward)] = typeof(Watermelon.NoAdsRewardView);
            map[typeof(Watermelon.CurrencyReward)] = typeof(Watermelon.CurrencyRewardView);
            map[typeof(Watermelon.SkinReward)] = typeof(Watermelon.SkinRewardView);

            return map;
        }
    }
}