using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum VillageKind {Settlement,City,TradeMetropole,PoliticsMetropole,ScienceMetropole};
enum ResourceKind {Wool,Lumber,Ore,Brick,Grain};
enum CommodityKind {Coin,Cloth,Paper};
enum ProgressCardKind {ArchemistCard
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

enum PlayerStatus {Offline,Available,InGame,Ready};

enum EventKind {Barbarian,Trade,Politics,Science};

enum GamePhase {ReadyToJoin,SetupRoundOne,SetupRoundTwo,TurnFirstPhase,TurnDiceRolled,TurnSecondPhase,Completed};

enum HarbourKind{None
	,Generic
	,Special};

enum TerrainKind{Sea
	,Desert
	,Pasture
	,Forest
	,Mountains
	,Hills
	,Fields
	,GoldMine};




public class Enums : Object {}
