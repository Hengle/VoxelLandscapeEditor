﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelLandscapeEditor
{
    public enum CursorMode
    {
        LandScape,
        Trees,
        Water
    }

    public class Cursor
    {
        public const int X_SIZE = 32, Y_SIZE = 32, Z_SIZE = 1;

        public Voxel[, ,] Voxels = new Voxel[X_SIZE, Y_SIZE, Z_SIZE];

        public List<VertexPositionNormalColor> Vertices = new List<VertexPositionNormalColor>();
        public List<short> Indexes = new List<short>();

        public VertexPositionNormalColor[] VertexArray;
        public short[] IndexArray;

        public Vector3 Position;
        public int Size = 5;

        public int Height = 5;

        public CursorMode Mode;

        public Cursor()
        {
            UpdateMesh();
        }

        public void Update(GameTime gameTime)
        {
            if (Height < 2) Height = 2;
            if (Height > 20) Height = 20;

            if ((int)Mode > 2) Mode = 0;
        }

        public void UpdateMesh()
        {
            for(int x=0;x<X_SIZE;x++) for(int y=0;y<Y_SIZE;y++) Voxels[x,y,0].Active = false;
            Vector2 center = new Vector2(X_SIZE,Y_SIZE)/2;

            if (Size < 1) Size = 1;
            if (Size > 15) Size = 15;

            for (float a = 0f; a < MathHelper.TwoPi; a += 0.01f)
            {
                for (int r = 0; r <Size; r++)
                {
                    Vector2 pos = Helper.PointOnCircle(ref center, r, a);
                    Voxels[(int)pos.X, (int)pos.Y, 0].Active = true;
                }
                //Voxels[(int)center.X, (int)center.Y, 0].Active = false;
            }

            Vector3 meshCenter = (new Vector3(X_SIZE, Y_SIZE, Z_SIZE) * Voxel.SIZE) / 2f;
            Vertices.Clear();
            Indexes.Clear();

            for(int x=0;x<X_SIZE;x++)
                for(int y=0;y<Y_SIZE;y++)
                    for (int z = 0; z < Z_SIZE; z++)
                    {
                        if (Voxels[x, y, z].Active == false) continue;

                        Vector3 worldOffset = ((new Vector3(x, y, z) * Voxel.SIZE)) - meshCenter;

                        MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(0f, 0f, -1f), Color.Red*0.5f);
                        //if (!IsVoxelAt(x, y, z + 1)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 0f, 1f), Voxels[x, y, z].Color);
                        MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-1f, 0f, 0f), Color.Red * 0.5f);
                        MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(1f, 0f, 0f), Color.Red * 0.5f);
                        MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 0f, 1f), Color.Red * 0.5f);
                        MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 0f, -1f), Color.Red * 0.5f); 
                    }

            VertexArray = Vertices.ToArray();
            IndexArray = new short[Indexes.Count];

            for (int ind = 0; ind < Indexes.Count / 6; ind++)
            {
                for (int i = 0; i < 6; i++)
                {
                    IndexArray[(ind * 6) + i] = (short)(Indexes[(ind * 6) + i] + (ind * 4));
                }
            }

            

            Vertices.Clear();
            Indexes.Clear();
        }

        public void PerformAction(World gameWorld)
        {
            // Need to make it so voxels have a type and then we can check if tree so we don't put trees on top of trees yo

            Vector2 center = new Vector2(Position.X, Position.Y);

            switch (Mode)
            {
                case CursorMode.LandScape:
                    for (float a = 0f; a < MathHelper.TwoPi; a += 0.05f)
                    {
                        for (int r = 0; r < Size; r++)
                        {
                            Vector3 pos = new Vector3(Helper.PointOnCircle(ref center, r, a), 0f);
                            for (int z = Chunk.Z_SIZE - 1; z >= 0; z--)
                            {
                                gameWorld.SetVoxel((int)pos.X, (int)pos.Y, z, z>=Chunk.Z_SIZE-Height, VoxelType.Ground, new Color(0f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0f), new Color(0f, 0.3f, 0f));
                            }
                        }
                    }
                    break;

                case CursorMode.Trees:
                    int numTrees = 1 +(Size / 2);
                    for (int i = 0; i < numTrees; i++)
                    {
                        Vector2 pos = Helper.RandomPointInCircle(new Vector2(Position.X,Position.Y), 0f, Size);

                        //int z = 0;
                        //for (int zz = 0; zz < Chunk.Z_SIZE; zz++) if (gameWorld.GetVoxel((int)Position.X, (int)Position.Y, zz).Active) { z = zz - 1; break; }

                        gameWorld.MakeTree((int)pos.X, (int)pos.Y, (int)Position.Z);
                    }

                    //for (float a = 0f; a < MathHelper.TwoPi; a += 0.05f)
                    //{
                    //    for (int r = 0; r < Size; r++)
                    //    {
                    //        Vector3 pos = new Vector3(Helper.PointOnCircle(ref center, r, a), 0f);
                    //        gameWorld.MakeTree((int)pos.X, (int)pos.Y, (int)Position.Z);
                    //    }
                    //}
                    break;
                case CursorMode.Water:
                    for (float a = 0f; a < MathHelper.TwoPi; a += 0.05f)
                    {
                        for (int r = 0; r < Size; r++)
                        {
                            Vector3 pos = new Vector3(Helper.PointOnCircle(ref center, r, a), 0f);
                            for (int z = Chunk.Z_SIZE - 1; z >= 0; z--)
                            {
                                gameWorld.SetVoxel((int)pos.X, (int)pos.Y, z, z >= Chunk.Z_SIZE - (Height-2), VoxelType.Ground, new Color(0f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0f), new Color(0f, 0.3f, 0f));
                            }
                            gameWorld.SetVoxel((int)pos.X, (int)pos.Y, (int)Position.Z , true, VoxelType.Water, new Color(0.1f, 0.1f, 0.8f + ((float)Helper.Random.NextDouble() * 0.1f)) * 0.8f, new Color(0.3f, 0.3f, 0.8f + ((float)Helper.Random.NextDouble() * 0.1f)) * 0.8f);
                        }
                    }
                    break;
            }
        }

        void MakeQuad(Vector3 offset, Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl, Vector3 norm, Color col)
        {
            Vertices.Add(new VertexPositionNormalColor(offset + tl, norm, col));
            Vertices.Add(new VertexPositionNormalColor(offset + tr, norm, col));
            Vertices.Add(new VertexPositionNormalColor(offset + br, norm, col));
            Vertices.Add(new VertexPositionNormalColor(offset + bl, norm, col));
            Indexes.Add(0);
            Indexes.Add(1);
            Indexes.Add(2);
            Indexes.Add(2);
            Indexes.Add(3);
            Indexes.Add(0);
        }
    }
}