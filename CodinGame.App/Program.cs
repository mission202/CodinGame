using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Made with love by AntiSquid, Illedan and Wildum.
 * You can help children learn to code while you participate by donating to CoderDojo.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        int myTeam = int.Parse(Console.ReadLine());
        int bushAndSpawnPointCount = int.Parse(Console.ReadLine()); // usefrul from wood1, represents the number of bushes and the number of places where neutral units can spawn
        for (int i = 0; i < bushAndSpawnPointCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            string entityType = inputs[0]; // BUSH, from wood1 it can also be SPAWN
            int x = int.Parse(inputs[1]);
            int y = int.Parse(inputs[2]);
            int radius = int.Parse(inputs[3]);
        }
        int itemCount = int.Parse(Console.ReadLine()); // useful from wood2
        for (int i = 0; i < itemCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            string itemName = inputs[0]; // contains keywords such as BRONZE, SILVER and BLADE, BOOTS connected by "_" to help you sort easier
            int itemCost = int.Parse(inputs[1]); // BRONZE items have lowest cost, the most expensive items are LEGENDARY
            int damage = int.Parse(inputs[2]); // keyword BLADE is present if the most important item stat is damage
            int health = int.Parse(inputs[3]);
            int maxHealth = int.Parse(inputs[4]);
            int mana = int.Parse(inputs[5]);
            int maxMana = int.Parse(inputs[6]);
            int moveSpeed = int.Parse(inputs[7]); // keyword BOOTS is present if the most important item stat is moveSpeed
            int manaRegeneration = int.Parse(inputs[8]);
            int isPotion = int.Parse(inputs[9]); // 0 if it's not instantly consumed
        }

        // game loop
        while (true)
        {
            int gold = int.Parse(Console.ReadLine());
            int enemyGold = int.Parse(Console.ReadLine());
            int roundType = int.Parse(Console.ReadLine()); // a positive value will show the number of heroes that await a command
            int entityCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int unitId = int.Parse(inputs[0]);
                int team = int.Parse(inputs[1]);
                string unitType = inputs[2]; // UNIT, HERO, TOWER, can also be GROOT from wood1
                int x = int.Parse(inputs[3]);
                int y = int.Parse(inputs[4]);
                int attackRange = int.Parse(inputs[5]);
                int health = int.Parse(inputs[6]);
                int maxHealth = int.Parse(inputs[7]);
                int shield = int.Parse(inputs[8]); // useful in bronze
                int attackDamage = int.Parse(inputs[9]);
                int movementSpeed = int.Parse(inputs[10]);
                int stunDuration = int.Parse(inputs[11]); // useful in bronze
                int goldValue = int.Parse(inputs[12]);
                int countDown1 = int.Parse(inputs[13]); // all countDown and mana variables are useful starting in bronze
                int countDown2 = int.Parse(inputs[14]);
                int countDown3 = int.Parse(inputs[15]);
                int mana = int.Parse(inputs[16]);
                int maxMana = int.Parse(inputs[17]);
                int manaRegeneration = int.Parse(inputs[18]);
                string heroType = inputs[19]; // DEADPOOL, VALKYRIE, DOCTOR_STRANGE, HULK, IRONMAN
                int isVisible = int.Parse(inputs[20]); // 0 if it isn't
                int itemsOwned = int.Parse(inputs[21]); // useful from wood1
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // If roundType has a negative value then you need to output a Hero name, such as "DEADPOOL" or "VALKYRIE".
            // Else you need to output roundType number of any valid action, such as "WAIT" or "ATTACK unitId"
            Console.WriteLine("WAIT");
        }
    }
}

public class Game
{
    // TODO: Init State

    // TODO: Get Next Move
}