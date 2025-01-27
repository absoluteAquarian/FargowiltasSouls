using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Projectiles.BossWeapons
{
    public class PhantasmalEyeHoming : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_452";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Phantasmal Eye");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.Homing[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.width = 12;
            projectile.height = 12;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = 2;
            projectile.timeLeft = 300;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.alpha = 0;
        }

        public override void AI()
        {
            if (--projectile.ai[1] < 0)
            {
                projectile.tileCollide = true;

                if (projectile.ai[0] == -1) //no target atm
                {
                    if (projectile.ai[1] % 6 == 0)
                    {
                        projectile.ai[0] = FargoSoulsUtil.FindClosestHostileNPC(projectile.Center, 2000, true);
                        projectile.netUpdate = true;
                        if (projectile.ai[0] == -1)
                        {
                            projectile.Kill();
                            return;
                        }
                    }
                }
                else //currently have target
                {
                    NPC npc = Main.npc[(int)projectile.ai[0]];

                    if (npc.active && npc.CanBeChasedBy()) //target is still valid
                    {
                        Vector2 distance = npc.Center - projectile.Center;
                        double angle = distance.ToRotation() - projectile.velocity.ToRotation();
                        if (angle > Math.PI)
                            angle -= 2.0 * Math.PI;
                        if (angle < -Math.PI)
                            angle += 2.0 * Math.PI;

                        float modifier = Math.Min(projectile.velocity.Length() / 100f, 1f);
                        projectile.velocity = projectile.velocity.RotatedBy(angle * modifier);
                    }
                    else //target lost, reset
                    {
                        projectile.ai[0] = -1;
                        projectile.netUpdate = true;
                    }
                }
            }

            if (projectile.ai[1] < 0)
                projectile.velocity = Vector2.Normalize(projectile.velocity) * MathHelper.Lerp(projectile.velocity.Length(), 24f, 0.02f);

            projectile.rotation = projectile.velocity.ToRotation() + 1.570796f;

            if (projectile.localAI[0] < ProjectileID.Sets.TrailCacheLength[projectile.type])
                projectile.localAI[0] += 0.1f;
            else
                projectile.localAI[0] = ProjectileID.Sets.TrailCacheLength[projectile.type];

            projectile.localAI[1] += 0.25f;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<Buffs.Masomode.CurseoftheMoon>(), 600);
            target.immune[projectile.owner] = 1;
            projectile.timeLeft = 0;
        }

        public override void Kill(int timeleft)
        {
            Main.PlaySound(SoundID.Zombie, (int)projectile.position.X, (int)projectile.position.Y, 103, 1f, 0.0f);
            projectile.position = projectile.Center;
            projectile.width = projectile.height = 144;
            projectile.position.X -= (float)(projectile.width / 2);
            projectile.position.Y -= (float)(projectile.height / 2);
            for (int index = 0; index < 2; ++index)
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 31, 0.0f, 0.0f, 100, new Color(), 1.5f);
            for (int index1 = 0; index1 < 20; ++index1)
            {
                int index2 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 229, 0.0f, 0.0f, 0, new Color(), 2.5f);
                Main.dust[index2].noGravity = true;
                Dust dust1 = Main.dust[index2];
                dust1.velocity = dust1.velocity * 3f;
                int index3 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 229, 0.0f, 0.0f, 100, new Color(), 1.5f);
                Dust dust2 = Main.dust[index3];
                dust2.velocity = dust2.velocity * 2f;
                Main.dust[index3].noGravity = true;
            }

            if (projectile.penetrate >= 0)
            {
                projectile.penetrate = -1;
                projectile.Damage();
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D glow = mod.GetTexture("Projectiles/MutantBoss/MutantEye_Glow");
            int rect1 = glow.Height / Main.projFrames[projectile.type];
            int rect2 = rect1 * projectile.frame;
            Rectangle glowrectangle = new Rectangle(0, rect2, glow.Width, rect1);
            Vector2 gloworigin2 = glowrectangle.Size() / 2f;
            Color glowcolor = Color.Lerp(new Color(31, 187, 192, 0), Color.Transparent, 0.74f);
            Vector2 drawCenter = projectile.Center - (projectile.velocity.SafeNormalize(Vector2.UnitX) * 14);

            for (int i = 0; i < 3; i++) //create multiple transparent trail textures ahead of the projectile
            {
                Vector2 drawCenter2 = drawCenter + (projectile.velocity.SafeNormalize(Vector2.UnitX) * 8).RotatedBy(MathHelper.Pi / 5 - (i * MathHelper.Pi / 5)); //use a normalized version of the projectile's velocity to offset it at different angles
                drawCenter2 -= (projectile.velocity.SafeNormalize(Vector2.UnitX) * 8); //then move it backwards
                float scale = projectile.scale;
                scale += (float)Math.Sin(projectile.localAI[1]) / 10;
                Main.spriteBatch.Draw(glow, drawCenter2 - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(glowrectangle),
                    glowcolor, projectile.velocity.ToRotation() + MathHelper.PiOver2, gloworigin2, scale, SpriteEffects.None, 0f);
            }

            for (float i = projectile.localAI[0] - 1; i > 0; i -= projectile.localAI[0] / 5) //trail grows in length as projectile travels
            {

                float lerpamount = 0.2f;
                if (i > 5 && i < 10)
                    lerpamount = 0.4f;
                if (i >= 10)
                    lerpamount = 0.6f;

                Color color27 = Color.Lerp(glowcolor, Color.Transparent, 0.1f + lerpamount);

                color27 *= ((int)((projectile.localAI[0] - i) / projectile.localAI[0]) ^ 2);
                float scale = projectile.scale * (float)(projectile.localAI[0] - i) / projectile.localAI[0];
                scale += (float)Math.Sin(projectile.localAI[1]) / 10;
                Vector2 value4 = projectile.oldPos[(int)i] - (projectile.velocity.SafeNormalize(Vector2.UnitX) * 14);
                Main.spriteBatch.Draw(glow, value4 + projectile.Size / 2f - Main.screenPosition + new Vector2(0, projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(glowrectangle), color27,
                    projectile.velocity.ToRotation() + MathHelper.PiOver2, gloworigin2, scale * 0.8f, SpriteEffects.None, 0f);
            }

            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture2D13 = Main.projectileTexture[projectile.type];
            int num156 = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            Main.spriteBatch.Draw(texture2D13, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), projectile.GetAlpha(lightColor), projectile.rotation, origin2, projectile.scale, SpriteEffects.None, 0f);

        }
    }
}