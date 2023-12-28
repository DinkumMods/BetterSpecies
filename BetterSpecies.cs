using BepInEx;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections.Generic;
using BepInEx.Logging;

namespace BetterSpecies {

	public static class Species {
		public enum Type : int {
			Human		= 0,
			Crocodile 	= 1,
			Shark 		= 2,
			BushDevil	= 3,
			Lizard 		= 4,
			Mu 			= 5
		}
		
		static Dictionary<int, Species.Type> species = new Dictionary<int, Species.Type>() { // Dynamically generate?
			{  3, Type.Crocodile },	// Croco
			{ 20, Type.Crocodile },	// Glowing Croco
			{ 25, Type.Crocodile }, // Alpha Croco
			{  5, Type.Shark }, 	// Shark
			{ 38, Type.Shark }, 	// Alpha Shark
			{ 14, Type.BushDevil }, // Bush Devil
			{ 26, Type.BushDevil }, // Alpha Bush Devil
			{ 28, Type.Lizard }, 	// Frilled Neck Lizard
			{ 31, Type.Lizard }, 	// Leaf Neck Lizard
			{ 13, Type.Mu }, 		// Mu
			{ 16, Type.Mu }, 		// Wary Mu
			{ 33, Type.Mu }, 		// Ridable Mu
		};
		
		public static bool isSameSpecies(int a, int b) {
			if (a == b)
				return true;
			
			if (species.TryGetValue(a, out Species.Type speciesA) && species.TryGetValue(b, out Species.Type speciesB)) {
				return speciesA == speciesB;
			}
			
			return false;
		}
	}
	
	[BepInAutoPlugin]
	public partial class Plugin : BaseUnityPlugin {
		private readonly Harmony harmony = new Harmony(Id);
		public static Plugin Instance;

		public static void Log(System.Object log) {
			Instance.Logger.LogInfo(log);
		}

		private void Awake() {
			Instance = this;
			harmony.PatchAll();
		}
		/*
		private void Start() {
			foreach (AnimalAI ai in AnimalManager.manage.allAnimals) {
				Log(ai);
			}
		}
		*/
	}
	
	[HarmonyPatch(typeof(AnimalAI_Attack), "returnClosestPrey")]
	public static class PreyTranspiler {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			CodeMatcher matcher = new CodeMatcher(instructions);
			
			matcher.MatchForward(false, 
				new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(AnimalAI), "animalId")),
				new CodeMatch(OpCodes.Ldarg_0),
				new CodeMatch(OpCodes.Ldfld), // myAI
				new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(AnimalAI), "animalId")),
				new CodeMatch(OpCodes.Beq)
			);
			
			if (matcher.IsValid) {
				matcher.Advance(4).InsertAndAdvance(CodeInstruction.Call(typeof(BetterSpecies.Species), "isSameSpecies"));
				matcher.Instruction.opcode = OpCodes.Brtrue;
				Plugin.Log("Patched returnClosestPrey");
			} else {
				Plugin.Log("Unable to patch returnClosestPrey");
			}

			return matcher.InstructionEnumeration();
		}
	}
}