﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BonVoyage
{
    class BonVoyageScenario : ScenarioModule
    {
        public static BonVoyageScenario Instance { get; private set; }

        private ConfigNode scenarioNode;


        /// <summary>
        /// Constructor
        /// </summary>
        public BonVoyageScenario()
        {
            Instance = this;
        }


        /// <summary>
        /// Load data
        /// </summary>
        /// <param name="gameNode"></param>
        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            scenarioNode = gameNode;
        }


        /// <summary>
        /// Save data
        /// </summary>
        /// <param name="gameNode"></param>
        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            SaveScenario(gameNode);
        }


        /// <summary>
        /// Save scenario details for each vessel with BV controller
        /// </summary>
        /// <param name="node"></param>
        private void SaveScenario(ConfigNode gameNode)
        {
            gameNode.ClearNodes();

			foreach(BVController controller in BonVoyage.Instance.BVControllers.Values)
			{
                if (controller.vessel == null)
                    continue;

                ConfigNode controllerNode = new ConfigNode("CONTROLLER");
                controllerNode.AddValue("vesselId", controller.vessel.id);

                ConfigNode subNode = new ConfigNode("BATTERIES");
                subNode.AddValue("useBatteries", controller.batteries.UseBatteries);
                subNode.AddValue("maxUsedEC", controller.batteries.MaxUsedEC);
                subNode.AddValue("ecPerSecondConsumed", controller.batteries.ECPerSecondConsumed);
                subNode.AddValue("ecPerSecondGenerated", controller.batteries.ECPerSecondGenerated);
                subNode.AddValue("currentEC", controller.batteries.CurrentEC);
                controllerNode.AddNode(subNode);

                subNode = new ConfigNode("FUEL_CELLS");
                subNode.AddValue("useFuelCells", controller.fuelCells.Use);
                subNode.AddValue("outputEC", controller.fuelCells.OutputValue);
                List<Resource> res = controller.fuelCells.InputResources;
                ConfigNode resourceNode;
                for (int r = 0; r < res.Count; r++)
                {
                    resourceNode = new ConfigNode("RESOURCE");
                    resourceNode.AddValue("name", res[r].Name);
                    resourceNode.AddValue("ratio", res[r].Ratio);
                    resourceNode.AddValue("maximumAmount", res[r].MaximumAmountAvailable);
                    resourceNode.AddValue("currentAmount", res[r].CurrentAmountUsed);
                    subNode.AddNode(resourceNode);
                }
                controllerNode.AddNode(subNode);

                subNode = new ConfigNode("PROPELLANTS");
                ConfigNode propellantNode;
                for (int r = 0; r < controller.propellants.Count; r++)
                {
                    propellantNode = new ConfigNode("FUEL");
                    propellantNode.AddValue("name", controller.propellants[r].Name);
                    propellantNode.AddValue("fuelFlow", controller.propellants[r].FuelFlow);
                    propellantNode.AddValue("maximumAmount", controller.propellants[r].MaximumAmountAvailable);
                    propellantNode.AddValue("currentAmount", controller.propellants[r].CurrentAmountUsed);
                    subNode.AddNode(propellantNode);
                }
                controllerNode.AddNode(subNode);

                gameNode.AddNode(controllerNode);
            }
        }


        /// <summary>
        /// Load scenario details for each vessel with BV controller
        /// </summary>
        public void LoadScenario()
        {
            if (scenarioNode != null)
            {
				foreach(BVController controller in BonVoyage.Instance.BVControllers.Values)
                {
                    if (controller.vessel == null)
                        continue;

                    ConfigNode controllerNode = scenarioNode.GetNode("CONTROLLER", "vesselId", controller.vessel.id.ToString());
                    if (controllerNode != null)
                    {
                        ConfigNode subNode = controllerNode.GetNode("BATTERIES");
                        if (subNode != null)
                        {
                            controller.batteries.UseBatteries = Convert.ToBoolean(subNode.GetValue("useBatteries"));
                            controller.batteries.MaxUsedEC = Convert.ToDouble(subNode.GetValue("maxUsedEC"));
                            controller.batteries.ECPerSecondConsumed = Convert.ToDouble(subNode.GetValue("ecPerSecondConsumed"));
                            controller.batteries.ECPerSecondGenerated = Convert.ToDouble(subNode.GetValue("ecPerSecondGenerated"));
                            controller.batteries.CurrentEC = Convert.ToDouble(subNode.GetValue("currentEC"));
                        }

                        subNode = controllerNode.GetNode("FUEL_CELLS");
                        if (subNode != null)
                        {
                            controller.fuelCells.Use = Convert.ToBoolean(subNode.GetValue("useFuelCells"));
                            controller.fuelCells.OutputValue = Convert.ToDouble(subNode.GetValue("outputEC"));
                            var resources = subNode.GetNodes("RESOURCE");
                            controller.fuelCells.InputResources.Clear();
                            for (int r = 0; r < resources.Length; r++)
                            {
                                Resource ir = new Resource();
                                ir.Name = resources[r].GetValue("name");
                                ir.Ratio = Convert.ToDouble(resources[r].GetValue("ratio"));
                                ir.MaximumAmountAvailable = Convert.ToDouble(resources[r].GetValue("maximumAmount"));
                                ir.CurrentAmountUsed = Convert.ToDouble(resources[r].GetValue("currentAmount"));
                                controller.fuelCells.InputResources.Add(ir);
                            }
                        }

                        subNode = controllerNode.GetNode("PROPELLANTS");
                        if (subNode != null)
                        {
                            var propellants = subNode.GetNodes("FUEL");
                            controller.propellants.Clear();
                            for (int r = 0; r < propellants.Length; r++)
                            {
                                Fuel ir = new Fuel();
                                ir.Name = propellants[r].GetValue("name");
                                ir.FuelFlow = Convert.ToDouble(propellants[r].GetValue("fuelFlow"));
                                ir.MaximumAmountAvailable = Convert.ToDouble(propellants[r].GetValue("maximumAmount"));
                                ir.CurrentAmountUsed = Convert.ToDouble(propellants[r].GetValue("currentAmount"));
                                controller.propellants.Add(ir);
                            }
                        }
                    }
                }
            }
        }

    }

}
