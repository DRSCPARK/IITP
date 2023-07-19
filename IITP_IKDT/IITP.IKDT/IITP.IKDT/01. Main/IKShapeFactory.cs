using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinokio._3D;
using Pinokio._3D.Eyeshot;
using Pinokio.Geometry;
using Pinokio.Map;
using Pinokio.Simulation;
using Pinokio.IKDT;
using Pinokio.Simulation.Models.IK;
using Pinokio.Simulation.FACTORY;

namespace Pinokio.IKDT
{
    public class IKShapeFactory : ShapeFactory
    {
        private EyeshotViewFrame _viewFrame;
        public IKShapeFactory(EyeshotViewFrame viewFrame)
        {
            _viewFrame = viewFrame;
            AddDrawSetting("Node", new EyeshotDrawSetting(true, "Node") { Size = new Vector3(50), MainColor = ColorDefinition.Basic });
        }

        public override void AddModelShapes(Dictionary<uint, SimModel> models)
        {
            foreach (SimModel model in models.Values)
            {
                EyeshotShapeType type = EyeshotShapeType.None;
                EyeshotDrawSetting drawSet = null;

                if (string.IsNullOrEmpty(model.Name) == false)
                {
                    switch (model.Name)
                    {
                        case "HANIL40T1":
                            type = EyeshotShapeType.Process;
                            break;
                        case "HANIL110T1":
                            type = EyeshotShapeType.Process;
                            break;
                        case "HANIL150T1":
                            type = EyeshotShapeType.Process;
                            break;
                        case "HANIL80T1":
                            type = EyeshotShapeType.Process;
                            break;
                        case "KYORI40T1":
                            type = EyeshotShapeType.Process;
                            break;
                        case "KYORI30T1":
                            type = EyeshotShapeType.Process;
                            break;
                        case "KYORI30T2":
                            type = EyeshotShapeType.Process;
                            break;
                        case "MITSUI60T1":
                            type = EyeshotShapeType.Process;
                            break;
                        default:
                            type = EyeshotShapeType.Process;
                            break;
                    }
                }

                if (model.AbstractObj != null && DrawSettings.ContainsKey(model.Name))
                {
                    drawSet = DrawSettings[model.Name] as EyeshotDrawSetting;
                }

                if (drawSet != null)
                {
                    Shape shape = _viewFrame.AddShape(model, type);
                    shape.SetDrawSetting(drawSet);
                }
            }
        }

        public void DefineDrawSettings(Factory facotry)
        {
            foreach (var eqpTup in facotry.Equipments)
            {
                /* 3D */
                EyeshotDrawSetting drawSet = new EyeshotDrawSetting(true, eqpTup.Key) { Size = eqpTup.Value.Size, MainColor = ColorDefinition.Commit, DrawByFile = true, FileType = FileType.Obj };
                switch (eqpTup.Value.ToolGroupName)
                {
                    case "HANIL40T":
                        drawSet.FileName = "HANIL40T";
                        break;
                    case "HANIL110T":
                        drawSet.FileName = "HANIL110T";
                        break;
                    case "HANIL150T":
                        drawSet.FileName = "HANIL150T";
                        break;
                    case "HANIL80T":
                        drawSet.FileName = "HANIL80T";
                        break;
                    case "KYORI40T":
                        drawSet.FileName = "KYORI40T";
                        break;
                    case "KYORI30T":
                        drawSet.FileName = "KYORI30T";
                        break;
                    case "MITSUI60T":
                        drawSet.FileName = "MITSUI60T";
                        break;
                }
                if (string.IsNullOrEmpty(drawSet.FileName) == false)
                    AddDrawSetting(eqpTup.Value.Name, drawSet);
            }
        }
    }
}
