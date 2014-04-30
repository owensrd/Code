using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Graph
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        public class graphNode
        {
            public Texture2D circle;
            public Vector2 circlePos;
            public Color circleCol;
            public int connector;
        }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        BasicEffect basicEffect;

        Vector2 oldMouse = new Vector2(0, 0);
        MouseState oldMe;
        KeyboardState oldKb;

        List<graphNode> Circle;

        Random r;

        int MAXLENGTH = 100;
        float attConst = 0.02f;
        float repConst = 500.0f;

        int nodeRadius = 20;

        SpriteFont Font1;
        Vector2 FontPos;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public static int CalcDistance(Vector2 a, Vector2 b)
        {
            double xDist = (a.X - b.X);
            double yDist = (a.Y - b.Y);
            return (int)Math.Sqrt(Math.Pow(xDist, 2) + Math.Pow(yDist, 2));
        }

        public void addNode()
        {
            if (Circle.Count == 0)
            {
                var cir = new graphNode();
                int X = r.Next(5, 755);
                int Y = r.Next(5, 435);
                cir.circle = CreateCircle(nodeRadius);
                cir.circlePos = new Vector2(X, Y);
                cir.circleCol = Color.Red;
                cir.connector = 1000;
                Circle.Add(cir);
            }
            else if (Circle.Count == 1)
            {
                var cir = new graphNode();
                int X = r.Next(5, 755);
                int Y = r.Next(5, 435);
                cir.circle = CreateCircle(nodeRadius);
                cir.circlePos = new Vector2(X, Y);
                cir.circleCol = Color.Red;
                cir.connector = 0;
                Circle.Add(cir);
            }
            else 
            {
                var cir = new graphNode();
                int X = r.Next(5, 755);
                int Y = r.Next(5, 435);
                int nodeConn = r.Next(0, Circle.Count);
                cir.circle = CreateCircle(nodeRadius);
                cir.circlePos = new Vector2(X, Y);
                cir.circleCol = Color.Red;
                cir.connector = nodeConn;
                Circle.Add(cir);
            }
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height, 0, 0, 1);

            Circle = new List<graphNode>();
            
            r = new Random();
            int count = r.Next(4, 10);
            for (int i = 0; i < count; i++)
            {
                addNode();
            }
            oldKb = Keyboard.GetState();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Font1 = Content.Load<SpriteFont>("SpriteFont1");
            FontPos = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2,
                graphics.GraphicsDevice.Viewport.Height / 2);
        }

        protected override void UnloadContent()
        {
        }

        public Texture2D CreateCircle(int radius)
        {
            int outerRadius = radius * 2 + 2;
            Texture2D texture = new Texture2D(GraphicsDevice, outerRadius, outerRadius);

            Color[] data = new Color[outerRadius * outerRadius];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }

            double angleStep = 1f / radius;

            for (double angle = 0; angle < Math.PI * 2; angle += angleStep)
            {
                int x = (int)Math.Round(radius + radius * Math.Cos(angle));
                int y = (int)Math.Round(radius + radius * Math.Sin(angle));

                data[y * outerRadius + x + 1] = Color.White;
            }

            for (int i = 0; i < outerRadius; i++)
            {
                int yStart = -1;
                int yEnd = -1;

                for (int j = 0; j < outerRadius; j++)
                {
                    if (yStart == -1)
                    {
                        if (j == outerRadius - 1)
                        {
                            break;
                        }
                        if (data[i + (j * outerRadius)] == Color.White && data[i + ((j + 1) * outerRadius)] == Color.Transparent)
                        {
                            yStart = j + 1;
                            continue;
                        }
                    }
                    else if (data[i + (j * outerRadius)] == Color.White)
                    {
                        yEnd = j;
                        break;
                    }
                }

                if (yStart != -1 && yEnd != -1)
                {
                    for (int j = yStart; j < yEnd; j++)
                    {
                        data[i + (j * outerRadius)] = Color.White;
                    }
                }
            }

            texture.SetData(data);
            return texture;

        }

        public Vector2 CalcRepulsionForce(Vector2 a, Vector2 b)
        {
            Vector2 result = new Vector2(0, 0);
            int proximity = CalcDistance(a, b);
            Vector2 btoa = a - b;
            btoa.Normalize();
            result = (repConst / (float)Math.Pow(proximity, 2)) * btoa;
            return result;
        }

        public Vector2 CalcAttractForce(Vector2 a, Vector2 b)
        {
            Vector2 result = new Vector2(0, 0);
            int proximity = CalcDistance(a, b);
            Vector2 btoa = a - b;
            btoa.Normalize();
            result = -(btoa * (proximity - MAXLENGTH));
            return result * attConst;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            MouseState me = Mouse.GetState();

            KeyboardState kb = Keyboard.GetState();

            if (oldKb.IsKeyDown(Keys.Space) && kb.IsKeyUp(Keys.Space))
            {
                addNode();
            }

            oldKb = kb;

            if (oldMouse.X == 0 && oldMouse.Y == 0)
            {
                oldMouse.X = me.X;
                oldMouse.Y = me.Y;
            }

            if (kb.IsKeyDown(Keys.C))
            {
                Circle.Clear();
            }

            if (kb.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            for (int i = 0; i < Circle.Count; i++)
            {
                Vector2 Dist;
                Dist.X = (Circle[i].circlePos.X + 20) - me.X;
                Dist.Y = (Circle[i].circlePos.Y + 20) - me.Y;
                if (Dist.Length() < nodeRadius)
                {

                    Circle[i].circleCol =  Color.Magenta;
                    if (me.LeftButton == ButtonState.Pressed)
                    {
                        Vector2 mPos = new Vector2(0, 0);
                        if (oldMe.X != me.X && oldMe.Y != me.Y)
                        {

                            mPos.X = me.X - oldMe.X;
                            mPos.Y = me.Y - oldMe.Y;
                        }
                        Circle[i].circlePos.X = Circle[i].circlePos.X + mPos.X;
                        Circle[i].circlePos.Y = Circle[i].circlePos.Y + mPos.Y;
                    }
                    else
                    {
                    }
                }
                else if (oldMe.LeftButton == ButtonState.Pressed && Circle[i].circleCol == Color.Magenta)
                {
                    Vector2 mPos = new Vector2(0, 0);
                    if (oldMe.X != me.X && oldMe.Y != me.Y)
                    {

                        mPos.X = me.X - oldMe.X;
                        mPos.Y = me.Y - oldMe.Y;
                    }
                    Circle[i].circlePos.X = Circle[i].circlePos.X + mPos.X;
                    Circle[i].circlePos.Y = Circle[i].circlePos.Y + mPos.Y;
                }
                else
                {
                    Circle[i].circleCol = Color.Red;
                }
            }
            
            if (me.LeftButton != ButtonState.Pressed)
            {
                for (int i = 0; i < Circle.Count; i++)
                {
                    Vector2 netForce = new Vector2(0, 0);
                    Vector2 attractForce = new Vector2(0, 0);
                    for (int j = 0; j < Circle.Count; j++)
                    {
                        if (j != i)
                        {
                            netForce = netForce + CalcRepulsionForce(Circle[i].circlePos, Circle[j].circlePos);
                        }
                    }
                    if (Circle[i].connector != 1000)
                    {
                        attractForce = CalcAttractForce(Circle[i].circlePos, Circle[Circle[i].connector].circlePos);
                        netForce = netForce + attractForce;
                        Circle[Circle[i].connector].circlePos = Circle[Circle[i].connector].circlePos - attractForce;
                    }
                    
                        
                    Circle[i].circlePos = Circle[i].circlePos + netForce;
                    if (Circle[i].circlePos.X < 5)
                        Circle[i].circlePos.X = 5;
                    if (Circle[i].circlePos.X > 755)
                        Circle[i].circlePos.X = 755;
                    if (Circle[i].circlePos.Y < 5)
                        Circle[i].circlePos.Y = 5;
                    if (Circle[i].circlePos.Y > 435)
                        Circle[i].circlePos.Y = 435;
                    if (Circle[i].connector != 1000)
                    {
                        if (Circle[Circle[i].connector].circlePos.X < 5)
                            Circle[Circle[i].connector].circlePos.X = 5;
                        if (Circle[Circle[i].connector].circlePos.X > 755)
                            Circle[Circle[i].connector].circlePos.X = 755;
                        if (Circle[Circle[i].connector].circlePos.Y < 5)
                            Circle[Circle[i].connector].circlePos.Y = 5;
                        if (Circle[Circle[i].connector].circlePos.Y > 435)
                            Circle[Circle[i].connector].circlePos.Y = 435;
                    }
                }
                
            }

            oldMe = me;

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();
            string output = "Controls:";
            Vector2 FontOrigin = Font1.MeasureString(output) / 2;
            FontPos = new Vector2(65, 15);
            spriteBatch.DrawString(Font1, output, FontPos, Color.LimeGreen, 0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);
            output = "Press SPACE to add a new node.";
            FontPos = new Vector2(65, 35);
            spriteBatch.DrawString(Font1, output, FontPos, Color.LightGreen, 0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);
            output = "Press C to clear all Nodes";
            FontPos = new Vector2(65, 55);
            spriteBatch.DrawString(Font1, output, FontPos, Color.LightGreen, 0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);
            output = "Press ESCAPE to exit.";
            FontPos = new Vector2(65, 75);
            spriteBatch.DrawString(Font1, output, FontPos, Color.LightGreen, 0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);
            basicEffect.CurrentTechnique.Passes[0].Apply();
            for (int i = 0; i < Circle.Count; i++)
            {
                if (Circle[i].connector != 1000)
                {
                    VertexPositionColor[] vert = new VertexPositionColor[6];
                    vert[0].Position = new Vector3(Circle[i].circlePos.X + nodeRadius, Circle[i].circlePos.Y + nodeRadius, 0);
                    vert[0].Color = Color.Black;
                    vert[1].Position = new Vector3(Circle[Circle[i].connector].circlePos.X + nodeRadius, Circle[Circle[i].connector].circlePos.Y + nodeRadius, 0);
                    vert[1].Color = Color.Black;
                    vert[2].Position = new Vector3(Circle[i].circlePos.X + nodeRadius, Circle[i].circlePos.Y + nodeRadius + 1, 0);
                    vert[2].Color = Color.Black;
                    vert[3].Position = new Vector3(Circle[Circle[i].connector].circlePos.X + nodeRadius, Circle[Circle[i].connector].circlePos.Y + nodeRadius + 1, 0);
                    vert[3].Color = Color.Black;
                    vert[4].Position = new Vector3(Circle[i].circlePos.X + nodeRadius, Circle[i].circlePos.Y + nodeRadius - 1, 0);
                    vert[4].Color = Color.Black;
                    vert[5].Position = new Vector3(Circle[Circle[i].connector].circlePos.X + nodeRadius, Circle[Circle[i].connector].circlePos.Y + nodeRadius - 1, 0);
                    vert[5].Color = Color.Black;
                    graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vert, 0, 3);
                }
                
            }
            
            for (int i = 0; i < Circle.Count; i++)
            {
                spriteBatch.Draw(Circle[i].circle, Circle[i].circlePos, Circle[i].circleCol);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
