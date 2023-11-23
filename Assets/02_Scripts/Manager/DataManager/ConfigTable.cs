#pragma warning disable 114
using System;
using System.Collections;
using System.Collections.Generic;
public class ConfigTable : Singleton<ConfigTable>{

	public int maxItemCount;
	public string test01;
	public int maxGold;
	public int StageStartCost;
	public int StaminaChargeTime;
	public int StaminaMaxCount;
	public int StaminaDefaultCount;
	public int GoldDefault;
	public int DefaultUnit01;
	public int WorldShipRewardCooltime;
	public int ShowDevilSayTextCoolTime;
	public int OfflineRewardMaxTime;
	public int OfflineRewardRate;
	public int FreeGachaCooltime;
	public void LoadConfig(Dictionary<string, Dictionary<string, object>> rowList)
	{
		foreach (var rowItem in rowList)
		{
			var field = typeof(ConfigTable).GetField(rowItem.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			field.SetValue(this, rowItem.Value["value"]);
		}
	}
};
