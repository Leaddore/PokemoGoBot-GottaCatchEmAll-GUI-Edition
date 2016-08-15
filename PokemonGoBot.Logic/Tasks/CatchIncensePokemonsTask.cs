﻿using System.Threading.Tasks;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using Logger = PokemonGoBot.Logic.Logging.Logger;
using LogLevel = PokemonGoBot.Logic.Logging.LogLevel;

namespace PokemonGoBot.Logic.Tasks
{
    public class CatchIncensePokemonsTask
    {
        public static async Task Execute()
        {
            if (!Logic._client.Settings.CatchIncensePokemon)
                return;

            var incensePokemon = await Logic._client.Map.GetIncensePokemons();

            if (incensePokemon.Result == GetIncensePokemonResponse.Types.Result.IncenseEncounterAvailable)
            {
                var pokemon = new MapPokemon
                {
                    EncounterId = incensePokemon.EncounterId,
                    ExpirationTimestampMs = incensePokemon.DisappearTimestampMs,
                    Latitude = incensePokemon.Latitude,
                    Longitude = incensePokemon.Longitude,
                    PokemonId = incensePokemon.PokemonId,
                    SpawnPointId = incensePokemon.EncounterLocation
                };

                if (Logic._client.Settings.UsePokemonToNotCatchList &&
                    Logic._client.Settings.PokemonsToNotCatch.Contains(pokemon.PokemonId))
                {
                    Logger.Write($"Ignore Pokemon - {pokemon.PokemonId} - is on ToNotCatch List", LogLevel.Debug);
                    return;
                }

                var encounter =
                    await Logic._client.Encounter.EncounterIncensePokemon(pokemon.EncounterId, pokemon.SpawnPointId);

                if (encounter.Result == IncenseEncounterResponse.Types.Result.IncenseEncounterSuccess)
                    await CatchPokemonTask.Execute(encounter, pokemon);
                else
                    Logger.Write($"Encounter problem: {encounter.Result}", LogLevel.Warning);
            }

            if (Logic._client.Settings.EvolvePokemon || Logic._client.Settings.EvolveOnlyPokemonAboveIV) await EvolvePokemonTask.Execute();
            if (Logic._client.Settings.TransferPokemon) await TransferPokemonTask.Execute();
        }
    }
}
