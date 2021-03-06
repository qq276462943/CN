﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Irelia : Champion
    {
        public Irelia()
        {
            SetSpells();
            LoadMenu();
        }
        private void SetSpells()
        {
            Q = new Spell(SpellSlot.Q, 650);

            W = new Spell(SpellSlot.W);

            E = new Spell(SpellSlot.E, 425);

            R = new Spell(SpellSlot.R, 1000);
            R.SetSkillshot(0, 80f, 1400f, false, SkillshotType.SkillshotLine);
        }

        private void LoadMenu()
        {
            //key
            var key = new Menu("热键", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招",true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "骚扰",true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LaneClearActive", "清线",true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitKey", "补兵",true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("技能", "SpellMenu");
            {
                var qMenu = new Menu("Q", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Q_Min_Distance", "Q最小范围",true).SetValue(new Slider(300, 0, 600)));
                    qMenu.AddItem(new MenuItem("Q_Gap_Close", "Q小兵突进",true).SetValue(true));
                    qMenu.AddItem(new MenuItem("Q_Under_Tower", "塔下Q敌人",true).SetValue(false));
                    spellMenu.AddSubMenu(qMenu);
                }

                var eMenu = new Menu("E", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("E_Only_Stun", "留E晕人",true).SetValue(true));
                    eMenu.AddItem(new MenuItem("E_Running", "E跑动敌人",true).SetValue(true));
                    spellMenu.AddSubMenu(eMenu);
                }

                var rMenu = new Menu("R", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("R_If_HP", "血量<X使用R",true).SetValue(new Slider(20)));
                    //rMenu.AddItem(new MenuItem("R_Wait_Sheen", "Wait for Sheen",true).SetValue(false));

                    spellMenu.AddSubMenu(rMenu);
                }

                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("selected", "锁定目标",true).SetValue(true));
                combo.AddItem(new MenuItem("UseQCombo", "使用Q",true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用W",true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用E",true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用R",true).SetValue(true));
                //add to menu
                menu.AddSubMenu(combo);
            }

            var harass = new Menu("骚扰", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用Q",true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "使用W",true).SetValue(false));
                harass.AddItem(new MenuItem("UseEHarass", "使用E",true).SetValue(true));
                AddManaManagertoMenu(harass, "Harass", 30);
                //add to menu
                menu.AddSubMenu(harass);
            }

            var lastHit = new Menu("补兵", "Lasthit");
            {
                lastHit.AddItem(new MenuItem("UseQLastHit", "使用Q",true).SetValue(true));
                AddManaManagertoMenu(lastHit, "Lasthit", 30);
                //add to menu
                menu.AddSubMenu(lastHit);
            }

            var farm = new Menu("清线", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用Q",true).SetValue(true));
                farm.AddItem(new MenuItem("UseQFarm_Tower", "塔下不Q",true).SetValue(true));
                farm.AddItem(new MenuItem("UseWFarm", "使用W",true).SetValue(true));
                farm.AddItem(new MenuItem("UseRFarm", "使用R",true).SetValue(true));
                AddManaManagertoMenu(farm, "LaneClear", 0);
                //add to menu
                menu.AddSubMenu(farm);
            }

            var miscMenu = new Menu("杂项", "Misc");
            {
                //miscMenu.AddItem(new MenuItem("Cast_EQ", "Cast EQ nearest target",true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                miscMenu.AddItem(new MenuItem("E_Gap_Closer", "使用E防突",true).SetValue(true));
                miscMenu.AddItem(new MenuItem("QE_Interrupt", "使用Q/E打断",true).SetValue(true));
                //add to menu
                menu.AddSubMenu(miscMenu);
            }

            var drawMenu = new Menu("显示", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "禁用",true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Q范围",true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_E", "E范围",true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "R范围",true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R_Killable", "显示R可杀标记",true).SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示连招伤害",true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示补充伤害",true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                drawMenu.AddItem(drawComboDamageMenu);
                drawMenu.AddItem(drawFill);
                DamageIndicator.DamageToUnit = GetComboDamage;
                DamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
                DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
                DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
                drawComboDamageMenu.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                    };
                drawFill.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                        DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                    };
                //add to menu
                menu.AddSubMenu(drawMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q);

            if (W.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.W) * 4;

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E);

            if (R.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R) * 4;

            comboDamage = ActiveItems.CalcDamage(target, comboDamage);

            return (float)(comboDamage + Player.GetAutoAttackDamage(target) * 4);
        }

        private float GetComboDmgPercent(Obj_AI_Hero target)
        {
            double comboDamage = GetComboDamage(target);

            var predHp = target.Health - comboDamage;
            var predHpPercent = predHp / target.MaxHealth * 100;

            return (float)predHpPercent;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            if (source == "Harass" && !HasMana("Harass"))
                return;

            if(useQ)
                Cast_Q();
            if(useW)
                Cast_W();
            //items
            if (source == "Combo")
            {
                var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                if (itemTarget != null)
                {
                    var dmg = GetComboDamage(itemTarget);
                    ActiveItems.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ActiveItems.KillableTarget = true;

                    ActiveItems.UseTargetted = true;
                }
            }
            if(useE)
                Cast_E();
            if(useR)
                Cast_R();
        }
        public void Lasthit()
        {
            if (menu.Item("UseQLastHit", true).GetValue<bool>() && HasMana("Lasthit"))
                Cast_Q_Last_Hit();
        }

        private void Farm()
        {
            if (!HasMana("LaneClear"))
                return;

            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionR = MinionManager.GetMinions(Player.ServerPosition, R.Range, MinionTypes.All,
                        MinionTeam.NotAlly);
            
            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();
            var useR = menu.Item("UseRFarm", true).GetValue<bool>();

            if (useQ)
                Cast_Q_Last_Hit();

            if (useW && allMinionsW.Count > 0 && W.IsReady())
                W.Cast();

            var rPred = R.GetLineFarmLocation(allMinionR);
            if (useR && rPred.MinionsHit > 0 && R.IsReady())
                R.Cast(rPred.Position);
        }

        private void Cast_Q()
        {
            var target = TargetSelector.GetTarget(Q.Range * 2, TargetSelector.DamageType.Physical);

            if (GetTargetFocus(Q.Range) != null)
                target = GetTargetFocus(Q.Range);

            if (Q.IsReady() && target != null)
            {
                if (Q.IsKillable(target))
                    Q.Cast(target, packets());

                if (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E) > target.Health)
                    Q.Cast(target);

                var minDistance = menu.Item("Q_Min_Distance", true).GetValue<Slider>().Value;

                if (!menu.Item("Q_Under_Tower", true).GetValue<bool>())
                    if (target.UnderTurret(true))
                        return;

                if (Player.Distance(target) > Q.Range / 2 && menu.Item("Q_Gap_Close", true).GetValue<bool>())
                {
                    var allMinionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);

                    Obj_AI_Base bestMinion = allMinionQ[0];

                    foreach (var minion in allMinionQ)
                    {
                        double dmg = 0;

                        dmg += Player.GetSpellDamage(minion, SpellSlot.Q);
                        if (W.IsReady() || Player.HasBuff("ireliahitenstylecharged", true))
                            dmg += Player.GetSpellDamage(minion, SpellSlot.W);

                        if (target.Distance(minion) < Q.Range && Player.Distance(minion) < Q.Range && target.Distance(minion) < target.Distance(Player) && dmg > minion.Health + 40)
                            if (target.Distance(minion) < target.Distance(bestMinion))
                                bestMinion = minion;
                    }

                    //check if can Q without activating
                    if (bestMinion != null)
                    {
                        if (target.Distance(bestMinion) < Q.Range && Player.Distance(bestMinion) < Q.Range)
                        {
                            var dmg2 = Player.GetSpellDamage(bestMinion, SpellSlot.Q);

                            if (dmg2 > bestMinion.Health + 40)
                            {
                                Q.Cast(bestMinion, packets());
                                return;
                            }

                            if (W.IsReady() || Player.HasBuff("ireliahitenstylecharged", true))
                                dmg2 += Player.GetSpellDamage(bestMinion, SpellSlot.W);

                            if (dmg2 > bestMinion.Health)
                            {
                                W.Cast(packets());
                                Q.Cast(bestMinion, packets());
                                return;
                            }
                        }
                    }
                }

                if (Player.Distance(target) > minDistance && Player.Distance(target) < Q.Range + target.BoundingRadius)
                {
                    Q.Cast(target, packets());
                }
            }
        }

        private void Cast_Q_Last_Hit()
        {
            var allMinionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Player.BoundingRadius, MinionTypes.All, MinionTeam.NotAlly);

            if (allMinionQ.Count > 0 && Q.IsReady())
            {

                foreach (var minion in allMinionQ)
                {
                    double dmg = Player.GetSpellDamage(minion, SpellSlot.Q);

                    if (Player.HasBuff("ireliahitenstylecharged", true))
                        dmg += Player.GetSpellDamage(minion, SpellSlot.W);


                    if (dmg > minion.Health + 35)
                    {
                        if (menu.Item("UseQFarm_Tower", true).GetValue<bool>())
                        {
                            if (!Utility.UnderTurret(minion, true))
                            {
                                Q.Cast(minion, packets());
                                return;
                            }
                        }
                        else
                            Q.Cast(minion, packets());
                    }
                }
            }
        }

        private void Cast_W()
        {
            var target = TargetSelector.GetTarget(200, TargetSelector.DamageType.Physical);

            if (GetTargetFocus(200) != null)
                target = GetTargetFocus(200);

            if (target != null && W.IsReady())
            {
                W.Cast(packets());
            }
        }

        private void Cast_E()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (GetTargetFocus(E.Range) != null)
                target = GetTargetFocus(E.Range);

            if (target != null && E.IsReady())
            {
                if (E.IsKillable(target))
                    E.Cast(target, packets());

                if (menu.Item("E_Only_Stun", true).GetValue<bool>())
                {
                    var targetHealthPercent = target.Health / target.MaxHealth * 100;

                    if (GetHealthPercent() < targetHealthPercent)
                    {
                        E.Cast(target, packets());
                        return;
                    }
                }

                if (menu.Item("E_Running", true).GetValue<bool>())
                {
                    var pred = Prediction.GetPrediction(target, 1f);

                    if (Player.Distance(target) < Player.Distance(pred.UnitPosition) && Player.Distance(target) > 200)
                        E.Cast(target, packets());
                }
            }
        }

        private void Cast_R()
        {
            var target = TargetSelector.GetTarget(Player.Spellbook.GetSpell(SpellSlot.R).ToggleState == 1 ? Q.Range : R.Range,
                TargetSelector.DamageType.Physical);

            var range = R.Range;
            if (GetTargetFocus(range) != null)
                target = GetTargetFocus(range);

            if (target != null && R.IsReady())
            {
                if (!Player.HasBuff("IreliaTranscendentBlades"))
                {
                    if (GetComboDmgPercent(target) < 25)
                        R.Cast(target, packets());

                    var rHpValue = menu.Item("R_If_HP", true).GetValue<Slider>().Value;
                    if (GetHealthPercent() <= rHpValue)
                        R.Cast(target, packets());
                }
                else if (Player.HasBuff("IreliaTranscendentBlades"))
                {
                    R.Cast(target, packets());
                }
            }
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("E_Gap_Closer", true).GetValue<bool>()) return;

            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                E.Cast(gapcloser.Sender, packets());
        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (spell.DangerLevel < InterruptableDangerLevel.Medium || unit.IsAlly)
                return;

            if (menu.Item("QE_Interrupt", true).GetValue<bool>())
            {
                var enemyHp = unit.Health / unit.MaxHealth * 100;
                if (GetHealthPercent() > enemyHp)
                    return;

                if (unit.IsValidTarget(E.Range))
                    E.Cast(unit, packets());

                if (unit.IsValidTarget(Q.Range))
                {
                    Q.Cast(unit, packets());
                    E.Cast(unit, packets());
                }
            }
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("LastHitKey", true).GetValue<KeyBind>().Active)
                    Lasthit();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_E", true).GetValue<bool>())
                if (E.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R_Killable", true).GetValue<bool>())
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(5000) && !x.IsDead && x.IsEnemy).OrderBy(x => x.Health))
                {
                    Vector2 wts = Drawing.WorldToScreen(target.Position);
                    if (GetComboDmgPercent(target) < 30 && R.IsReady())
                    {
                        Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "KILL!!!");

                    }

                    var enemyhp = target.Health / target.MaxHealth * 100;
                    if (GetHealthPercent() < enemyhp && E.IsReady())
                        Drawing.DrawText(wts[0] - 20, wts[1] - 30, Color.White, "Stunnable");
                }
            }
        }
    }
}
