using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VillageKind {Settlement,City,TradeMetropole,PoliticsMetropole,ScienceMetropole};
public enum ResourceKind {Wool,Lumber,Ore,Brick,Grain};
public enum CommodityKind {Coin,Cloth,Paper};
public enum ProgressCardKind {ArchemistCard
	,CraneCard
	,EngineerCard
	,InventorCard
	,IrrigationCard
	,MedicineCard
	,MiningCard
	,PrinterCard
	,RoadBuildingCard
	,SmithCard
	,BishopCard
	,ConstitutionCard
	,DeserterCard
	,DiplomatCard
	,IntrigueCard
	,SaboteurCard
	,SpyCard
	,WarlordCard
	,WeddingCard
	,ComercialHarborCard
	,MasterMerchantCard
	,MerchantCard
	,MerchantFleetCard
	,ResourceMonopolyCard
	,TradeMonopolyCard};

public enum PlayerStatus {Offline,Available,InGame,Ready};

public enum EventKind {Barbarian,Trade,Politics,Science};

public enum GamePhase {ReadyToJoin,SetupRoundOne,SetupRoundTwo,TurnFirstPhase,TurnDiceRolled,TurnSecondPhase,Completed};

public enum HarbourKind{None
	,Generic
	,Special};

public enum TerrainKind{Sea
	,Desert
	,Pasture
	,Forest
	,Mountains
	,Hills
	,Fields
	,GoldMine};




public class Enums : Object {}
