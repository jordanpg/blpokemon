function clientsCanBattle(%a, %b)
{
	if(!isObject(%clientA) || !isObject(%clientB))
		return false;

	if(%clientA.getNumPartyPokemon() <= 0 || %clientB.getNumPartyPokemon() <= 0)
		return false;

	return true;
}

function Pokemon_InitiateClientBattle(%clientA, %clientB, %type, %stage)
{
	if(!clientsCanBattle(%clientA, %clientB))
		return false;

	%battle = Pokemon_InitBattle(false);

	%combA = %clientA.getPartyPokemon(0);
	%combB = %clientB.getPartyPokemon(0);

	%r = %battle.setCombatants(%combA, %combB);

	if(!%r)
	{
		%battle.delete();
		return false;
	}

	%clientA.battle = %battle;
	%clientB.battle = %battle;

	%battle.clients = %clientA SPC %clientB;

	//Forcing zero for now because only single battles are possible.
	commandToClient(%clientA, 'Pokemon_InitBattle', 0, %stage);
	commandToClient(%clientB, 'Pokemon_InitBattle', 0, %stage);

	%clientA.waitType = 0;
	%clientB.waitType = 0;

	%battle.waitingClients = 2;
}

function serverCmdPokemon_BattleReady(%this, %type)
{
	if(!isObject(%this.battle))
		return;

	switch(%type)
	{
		case 0:
			if(%this.waitType != %type)
				return;

			%battle.waitingClients--;
			%this.waitType = -1;

			if(%battle.waitingClients == 0)
			{
				//send client more info
			}
	}
}

function GameConnection::pushPokemon(%this, %pokemon, %side, %ind)
{
	if(!isPokemon(%pokemon))
		return;

	%dex = %pokemon.data.dexNum;
	%level = %pokemon.getStat("Level");
	%name = %pokemon.nickname;
	%gender = %pokemon.gender - 1;
	%shiny = %pokemon.shiny;
	%id = %pokemon.getID();

	if(%side)
	{
		%hp = %pokemon.getStat("HP"); / %pokemon.getStat("HPMax");
		%hpmax = "";
	}
	else
	{
		%hp = %pokemon.getStat("HP");
		%hpmax = %pokemon.getStat("HPMax");

		%xp = %pokemon.getStat("XP");
	}

	commandToClient(%this, 'Pokeon_SetPokemon', %side, %ind, %dex, %level, %hp, %hpmax, %xp, %name, %gender, %shiny, %id);
}

function PokemonBattle::pushCombatants(%this)
{
	for(%i = 0; %i < %this.combatants; %i++)
	{
		%comb = %this.combatant[%i];

		%bl_id = %comb.owner;
		%obj = findClientByBL_ID(%bl_id);
		%posid = mFloor(%this.findCombatant(%comb) / 2); //Placement is determined by combatant index in an alternating pattern so that one team's pokemon will always be on every other index.
														 //ie team A's pokemon's indices could be 0, 2, and 4 in a triple battle, and team B's would be 1, 3, and 5.
														 //As such, we can divide the combatant index by two and round down to determine the placement ID of a specific combatant.

		//If the pokemon's owner is present, they are participating in the battle and we should send the client relevant information.
		if(isObject(%obj) || (%ind = searchWords(%this.clients, %obj)) != -1)
			%obj.pushPokemon(%comb, 0, %posid);

		//If there is an opposing player, we need to also send them some data, but we want to send it as a pokemon on the opposing team instead.
		if(isObject(%obj2 = getWord(%this.clients, !%ind))) //There will only ever be two clients battling, so we can count on the index being either zero or one.
			%obj2.pushPokemon(%comb, 1, %posid);			//Since this is the case, we can easily find the opposite client by doing a not operation on the word index cooresponding to a client in the list.
	}
}