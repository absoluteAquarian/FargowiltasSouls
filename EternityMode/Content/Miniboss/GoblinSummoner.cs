﻿using FargowiltasSouls.Buffs.Masomode;
using FargowiltasSouls.EternityMode.NPCMatching;
using FargowiltasSouls.Items.Accessories.Masomode;
using FargowiltasSouls.NPCs;
using FargowiltasSouls.Projectiles.Masomode;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.EternityMode.Content.Miniboss
{
    public class GoblinSummoner : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.GoblinSummoner);

        public int AttackTimer;

        public override void AI(NPC npc)
        {
            base.AI(npc);

            EModeGlobalNPC.Aura(npc, 200, ModContent.BuffType<Shadowflame>(), false, DustID.Shadowflame);
            if (++AttackTimer > 180)
            {
                AttackTimer = 0;
                Main.PlaySound(SoundID.Item8, npc.Center);
                if (npc.HasPlayerTarget && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 spawnPos = npc.Center + new Vector2(200f, 0f).RotatedBy(Math.PI / 2 * (i + 0.5));
                        //Vector2 speed = Vector2.Normalize(Main.player[npc.target].Center - spawnPos) * 10f;
                        int n = NPC.NewNPC((int)spawnPos.X, (int)spawnPos.Y, NPCID.ChaosBall);
                        if (n != 200 && Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                        for (int j = 0; j < 20; j++)
                        {
                            int d = Dust.NewDust(spawnPos, 0, 0, DustID.Shadowflame);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].scale += 0.5f;
                            Main.dust[d].velocity *= 6f;
                        }
                    }
                }
            }
        }

        public override void ModifyHitByAnything(NPC npc, Player player, ref int damage, ref float knockback, ref bool crit)
        {
            base.ModifyHitByAnything(npc, player, ref damage, ref knockback, ref crit);

            if (Main.rand.NextBool(3) && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 vel = new Vector2(9f, 0f).RotatedByRandom(2 * Math.PI);
                for (int i = 0; i < 6; i++)
                {
                    Vector2 speed = vel.RotatedBy(2 * Math.PI / 6 * (i + Main.rand.NextDouble() - 0.5));
                    float ai1 = Main.rand.Next(10, 80) * (1f / 1000f);
                    if (Main.rand.NextBool())
                        ai1 *= -1f;
                    float ai0 = Main.rand.Next(10, 80) * (1f / 1000f);
                    if (Main.rand.NextBool())
                        ai0 *= -1f;
                    Projectile.NewProjectile(npc.Center, speed, ModContent.ProjectileType<ShadowflameTentacleHostile>(), npc.damage / 4, 0f, Main.myPlayer, ai0, ai1);
                }
            }
        }

        public override bool CheckDead(NPC npc)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 50; i++)
                {
                    Projectile.NewProjectile(npc.Center, new Vector2(Main.rand.Next(-500, 501) / 100f, Main.rand.Next(-1000, 1) / 100f),
                        ModContent.ProjectileType<GoblinSpikyBall>(), npc.damage / 8, 0, Main.myPlayer);
                }
            }

            return base.CheckDead(npc);
        }

        public override void NPCLoot(NPC npc)
        {
            base.NPCLoot(npc);

            if (Main.rand.NextBool(5))
                Item.NewItem(npc.Hitbox, ModContent.ItemType<WretchedPouch>());
        }
    }
}
