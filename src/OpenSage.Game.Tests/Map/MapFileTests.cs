﻿using System.IO;
using System.Runtime.CompilerServices;
using OpenSage.Data.Map;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Data.Tests.Map
{
    public class MapFileTests
    {
        private readonly ITestOutputHelper _output;

        public MapFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanRoundtripMaps()
        {
            InstalledFilesTestData.ReadFiles(".map", _output, entry =>
            {
                // These maps have false positive differences, so ignore differences.
                bool skipRoundtripEqualityTest = false;
                switch (entry.FilePath)
                {
                    // Differences in passability data, because the original file appears to have
                    // un-initialized (random) values for partial passability bytes beyond the map width.
                    case @"Maps\USA07-TaskForces\USA07-TaskForces.map":
                    case @"maps\map evil shelobslair\map evil shelobslair.map": // BFME
                    case @"maps\map good shelobslair\map good shelobslair.map": // BFME
                    case @"maps\map mp argonath\map mp argonath.map": // BFME II
                        skipRoundtripEqualityTest = true;
                        break;
                }

                using (var entryStream = entry.Open())
                {
                    TestUtility.DoRoundtripTest(
                        () => MapFile.Decompress(entryStream),
                        stream => MapFile.FromStream(stream),
                        (mapFile, stream) => mapFile.WriteTo(stream),
                        skipRoundtripEqualityTest);
                }
            });
        }

        [Fact]
        public void BlendTileData_SingleTexture_128_NoBlending()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(1, mapFile.BlendTileData.Textures.Length);
            Assert.Equal("AsphaltType1", mapFile.BlendTileData.Textures[0].Name);
            Assert.Equal(0u, mapFile.BlendTileData.Textures[0].CellStart);
            Assert.Equal(4u, mapFile.BlendTileData.Textures[0].CellCount);
            Assert.Equal(2u, mapFile.BlendTileData.Textures[0].CellSize);

            for (var y = 0; y < mapFile.HeightMapData.Height; y++)
            {
                for (var x = 0; x < mapFile.HeightMapData.Width; x++)
                {
                    Assert.Equal(0, mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex);
                }
            }
        }

        [Fact]
        public void BlendTileData_SingleTexture_384_NoBlending()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(2, mapFile.BlendTileData.Textures.Length);

            Assert.Equal("CliffLargeType6", mapFile.BlendTileData.Textures[0].Name);
            Assert.Equal(0u, mapFile.BlendTileData.Textures[0].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[0].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[0].CellSize);

            for (var y = 0; y < mapFile.HeightMapData.Height; y++)
            {
                for (var x = 0; x < mapFile.HeightMapData.Width; x++)
                {
                    Assert.Equal(0, mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex);
                }
            }
        }

        [Fact]
        public void BlendTileData_TwoTextures_384_128_NoBlending()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(2, mapFile.BlendTileData.Textures.Length);

            Assert.Equal("CliffLargeType2", mapFile.BlendTileData.Textures[0].Name);
            Assert.Equal(0u, mapFile.BlendTileData.Textures[0].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[0].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[0].CellSize);

            Assert.Equal("AsphaltType1", mapFile.BlendTileData.Textures[1].Name);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[1].CellStart);
            Assert.Equal(4u, mapFile.BlendTileData.Textures[1].CellCount);
            Assert.Equal(2u, mapFile.BlendTileData.Textures[1].CellSize);

            for (var y = 0; y < mapFile.HeightMapData.Height; y++)
            {
                for (var x = 0; x < mapFile.HeightMapData.Width; x++)
                {
                    var textureIndex = x == 1 && y == 0 ? 1 : 0;
                    Assert.Equal(textureIndex, mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex);
                }
            }
        }

        [Fact]
        public void BlendTileData_MultipleTextures_TwoWayBlending()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(9, mapFile.BlendTileData.Textures.Length);

            Assert.Equal("AsphaltType1", mapFile.BlendTileData.Textures[0].Name);
            Assert.Equal(0u, mapFile.BlendTileData.Textures[0].CellStart);
            Assert.Equal(4u, mapFile.BlendTileData.Textures[0].CellCount);
            Assert.Equal(2u, mapFile.BlendTileData.Textures[0].CellSize);

            Assert.Equal("SandLargeType3Light", mapFile.BlendTileData.Textures[1].Name);
            Assert.Equal(4u, mapFile.BlendTileData.Textures[1].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[1].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[1].CellSize);

            Assert.Equal("CliffLargeType8", mapFile.BlendTileData.Textures[2].Name);
            Assert.Equal(40u, mapFile.BlendTileData.Textures[2].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[2].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[2].CellSize);

            Assert.Equal("GrassLargeType1", mapFile.BlendTileData.Textures[3].Name);
            Assert.Equal(76u, mapFile.BlendTileData.Textures[3].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[3].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[3].CellSize);

            Assert.Equal("SnowLargeType1", mapFile.BlendTileData.Textures[4].Name);
            Assert.Equal(112u, mapFile.BlendTileData.Textures[4].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[4].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[4].CellSize);
        }

        [Fact]
        public void BlendTileData_ThreeWayBlending()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(3, mapFile.BlendTileData.Textures.Length);

            Assert.Equal("AsphaltType1", mapFile.BlendTileData.Textures[0].Name);
            Assert.Equal(0u, mapFile.BlendTileData.Textures[0].CellStart);
            Assert.Equal(4u, mapFile.BlendTileData.Textures[0].CellCount);
            Assert.Equal(2u, mapFile.BlendTileData.Textures[0].CellSize);

            Assert.Equal("SandLargeType3Light", mapFile.BlendTileData.Textures[1].Name);
            Assert.Equal(4u, mapFile.BlendTileData.Textures[1].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[1].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[1].CellSize);

            Assert.Equal("CliffLargeType3b", mapFile.BlendTileData.Textures[2].Name);
            Assert.Equal(40u, mapFile.BlendTileData.Textures[2].CellStart);
            Assert.Equal(36u, mapFile.BlendTileData.Textures[2].CellCount);
            Assert.Equal(6u, mapFile.BlendTileData.Textures[2].CellSize);

            for (var y = 0; y < mapFile.HeightMapData.Height; y++)
            {
                for (var x = 0; x < mapFile.HeightMapData.Width; x++)
                {
                    int textureIndex;
                    if (y == 1 && (x == 1 || x == 2 || x == 3 || x == 4))
                        textureIndex = 1;
                    else if (y == 3 && (x == 1 || x == 2 || x == 3 || x == 4))
                        textureIndex = 2;
                    else
                        textureIndex = 0;
                    Assert.Equal(textureIndex, mapFile.BlendTileData.TextureIndices[mapFile.BlendTileData.Tiles[x, y]].TextureIndex);

                    if (y == 2 && (x == 1 || x == 2 || x == 3 || x == 4))
                    {
                        Assert.NotEqual(0, mapFile.BlendTileData.ThreeWayBlends[x, y]);
                    }
                    else
                    {
                        Assert.Equal(0, mapFile.BlendTileData.ThreeWayBlends[x, y]);
                    }

                    Assert.Equal(0, mapFile.BlendTileData.CliffTextures[x, y]);
                }
            }

            void assertBlend(int x, int y, int secondaryTextureIndex, BlendDirection direction, bool reversed = false)
            {
                var blendIndex = mapFile.BlendTileData.Blends[x, y];

                var blend = mapFile.BlendTileData.BlendDescriptions[blendIndex - 1];

                Assert.Equal(secondaryTextureIndex, mapFile.BlendTileData.TextureIndices[(int) blend.SecondaryTextureTile].TextureIndex);

                Assert.Equal(direction, blend.BlendDirection);

                Assert.Equal(reversed, blend.Flags.HasFlag(BlendFlags.Flipped));
            }

            void assertBlends(int startY, int textureIndex, bool includeBottom)
            {
                if (includeBottom)
                {
                    assertBlend(0, startY + 0, textureIndex, BlendDirection.BlendTowardsTopRight);
                    assertBlend(1, startY + 0, textureIndex, BlendDirection.BlendTowardsTop);
                    assertBlend(2, startY + 0, textureIndex, BlendDirection.BlendTowardsTop);
                    assertBlend(3, startY + 0, textureIndex, BlendDirection.BlendTowardsTop);
                    assertBlend(4, startY + 0, textureIndex, BlendDirection.BlendTowardsTop);
                    assertBlend(5, startY + 0, textureIndex, BlendDirection.BlendTowardsTopLeft);
                }

                assertBlend(0, startY + 1, textureIndex, BlendDirection.BlendTowardsRight);
                assertBlend(5, startY + 1, textureIndex, BlendDirection.BlendTowardsRight, true);

                assertBlend(0, startY + 2, textureIndex, BlendDirection.BlendTowardsTopRight, true);
                assertBlend(1, startY + 2, textureIndex, BlendDirection.BlendTowardsTop, true);
                assertBlend(2, startY + 2, textureIndex, BlendDirection.BlendTowardsTop, true);
                assertBlend(3, startY + 2, textureIndex, BlendDirection.BlendTowardsTop, true);
                assertBlend(4, startY + 2, textureIndex, BlendDirection.BlendTowardsTop, true);
                assertBlend(5, startY + 2, textureIndex, BlendDirection.BlendTowardsTopLeft, true);
            }

            assertBlends(0, 1, true);
            assertBlends(2, 2, false);

            void assertThreeWayBlend(int x, int y, int secondaryTextureIndex, BlendDirection direction)
            {
                var threeWayBlend = mapFile.BlendTileData.ThreeWayBlends[x, y];

                var blendDescription = mapFile.BlendTileData.BlendDescriptions[threeWayBlend - 1];

                Assert.Equal(secondaryTextureIndex, mapFile.BlendTileData.TextureIndices[(int) blendDescription.SecondaryTextureTile].TextureIndex);

                Assert.Equal(direction, blendDescription.BlendDirection);
            }

            assertThreeWayBlend(1, 2, 2, BlendDirection.BlendTowardsTop);
            assertThreeWayBlend(2, 2, 2, BlendDirection.BlendTowardsTop);
            assertThreeWayBlend(3, 2, 2, BlendDirection.BlendTowardsTop);
            assertThreeWayBlend(4, 2, 2, BlendDirection.BlendTowardsTop);
        }

        [Fact]
        public void BlendTileData_CliffTextures()
        {
            var mapFile = GetMapFile();

            Assert.Equal(196u, mapFile.BlendTileData.NumTiles);

            Assert.Equal(2, mapFile.BlendTileData.Textures.Length);
        }

        [Fact]
        public void WorldInfo()
        {
            var mapFile = GetMapFile();

            //Assert.Equal(MapCompressionType.RefPack, mapFile.WorldInfo.Compression);
            //Assert.Equal("FooBar", mapFile.WorldInfo.MapName);
            //Assert.Equal(MapWeatherType.Snowy, mapFile.WorldInfo.Weather);
        }

        [Fact]
        public void SidesList()
        {
            var mapFile = GetMapFile();

            Assert.Equal(5, mapFile.SidesList.Players.Length);

            //var player0 = mapFile.SidesList.Players[0];
            //Assert.Equal("", player0.Name);
            //Assert.Equal(false, player0.IsHuman);
            //Assert.Equal("Neutral", player0.DisplayName);
            //Assert.Equal("", player0.Faction);
            //Assert.Equal("", player0.Allies);
            //Assert.Equal("", player0.Enemies);
            //Assert.Equal(null, player0.Color);

            //var player1 = mapFile.SidesList.Players[1];
            //Assert.Equal("Human_Player", player1.Name);
            //Assert.Equal(true, player1.IsHuman);
            //Assert.Equal("Human Player", player1.DisplayName);
            //Assert.Equal("FactionAmerica", player1.Faction);
            //Assert.Equal("", player1.Allies);
            //Assert.Equal("", player1.Enemies);
            //Assert.Equal(new MapColorArgb(0xFF, 0x96, 0, 0xC8), player1.Color);

            //var player2 = mapFile.SidesList.Players[2];
            //Assert.Equal("Computer_Player_1", player2.Name);
            //Assert.Equal(false, player2.IsHuman);
            //Assert.Equal("Computer Player 1", player2.DisplayName);
            //Assert.Equal("FactionChina", player2.Faction);
            //Assert.Equal("Human_Player", player2.Allies);
            //Assert.Equal("Computer_Player_2 Computer_Player_3", player2.Enemies);
            //Assert.Equal(null, player2.Color);

            //var player3 = mapFile.SidesList.Players[3];
            //Assert.Equal("Computer_Player_2", player3.Name);
            //Assert.Equal(false, player3.IsHuman);
            //Assert.Equal("Computer Player 2", player3.DisplayName);
            //Assert.Equal("FactionGLA", player3.Faction);
            //Assert.Equal("", player3.Allies);
            //Assert.Equal("", player3.Enemies);
            //Assert.Equal(null, player3.Color);

            //var player4 = mapFile.SidesList.Players[4];
            //Assert.Equal("Computer_Player_3", player4.Name);
            //Assert.Equal(false, player4.IsHuman);
            //Assert.Equal("Computer Player 3", player4.DisplayName);
            //Assert.Equal("FactionGLADemolitionGeneral", player4.Faction);
            //Assert.Equal("", player4.Allies);
            //Assert.Equal("", player4.Enemies);
            //Assert.Equal(new MapColorArgb(0xFF, 0xFF, 0x96, 0xFF), player4.Color);

            Assert.Equal(11, mapFile.SidesList.Teams.Length);
        }

        [Fact]
        public void SidesList_Scripts()
        {
            GetMapFile();
        }

        [Fact]
        public void BlendTileData_Passability()
        {
            var mapFile = GetMapFile();

            Assert.Equal(false, mapFile.BlendTileData.Impassability[0, 0]);
            Assert.Equal(false, mapFile.BlendTileData.Impassability[0, 1]);
            Assert.Equal(true, mapFile.BlendTileData.Impassability[1, 0]);
            Assert.Equal(false, mapFile.BlendTileData.Impassability[1, 1]);

            Assert.Equal(false, mapFile.BlendTileData.ImpassabilityToPlayers[0, 0]);
            Assert.Equal(true, mapFile.BlendTileData.ImpassabilityToPlayers[0, 1]);
            Assert.Equal(false, mapFile.BlendTileData.ImpassabilityToPlayers[1, 0]);
            Assert.Equal(false, mapFile.BlendTileData.ImpassabilityToPlayers[1, 1]);

            Assert.Equal(true, mapFile.BlendTileData.PassageWidths[0, 0]);
            Assert.Equal(false, mapFile.BlendTileData.PassageWidths[0, 1]);
            Assert.Equal(false, mapFile.BlendTileData.PassageWidths[1, 0]);
            Assert.Equal(true, mapFile.BlendTileData.PassageWidths[1, 1]);

            Assert.Equal(false, mapFile.BlendTileData.Taintability[0, 0]);
            Assert.Equal(true, mapFile.BlendTileData.Taintability[0, 1]);
            Assert.Equal(true, mapFile.BlendTileData.Taintability[1, 0]);
            Assert.Equal(false, mapFile.BlendTileData.Taintability[1, 1]);

            Assert.Equal(true, mapFile.BlendTileData.ExtraPassability[0, 0]);
            Assert.Equal(false, mapFile.BlendTileData.ExtraPassability[0, 1]);
            Assert.Equal(false, mapFile.BlendTileData.ExtraPassability[1, 0]);
            Assert.Equal(false, mapFile.BlendTileData.ExtraPassability[1, 1]);
        }

        [Fact]
        public void BlendTileData_Flammability()
        {
            var mapFile = GetMapFile();

            Assert.Equal(TileFlammability.FireResistant, mapFile.BlendTileData.Flammability[0, 0]);
            Assert.Equal(TileFlammability.HighlyFlammable, mapFile.BlendTileData.Flammability[0, 1]);
            Assert.Equal(TileFlammability.Grass, mapFile.BlendTileData.Flammability[1, 0]);
            Assert.Equal(TileFlammability.Undefined, mapFile.BlendTileData.Flammability[1, 1]);
        }

        [Fact]
        public void BlendTileData_StandingWaterAreas()
        {
            var mapFile = GetMapFile();

            Assert.Equal(2, mapFile.StandingWaterAreas.Areas.Length);

            var standingWaterArea0 = mapFile.StandingWaterAreas.Areas[0];

            Assert.Equal(1u, standingWaterArea0.UniqueID);
            Assert.Equal("Pacific Ocean", standingWaterArea0.Name);
            Assert.Equal(string.Empty, standingWaterArea0.LayerName);
            Assert.Equal(0.07f, standingWaterArea0.UvScrollSpeed, 2);
            Assert.Equal(true, standingWaterArea0.UseAdditiveBlending);
            Assert.Equal("WaterRippleBump2.tga", standingWaterArea0.BumpMapTexture);
            Assert.Equal("SkyEnv2.tga", standingWaterArea0.SkyTexture);
            Assert.Equal(7, standingWaterArea0.Points.Length);
            Assert.Equal(2u, standingWaterArea0.WaterHeight);
            Assert.Equal(string.Empty, standingWaterArea0.FxShader);
            Assert.Equal("LUTDepthTint.tga", standingWaterArea0.DepthColors);

            var standingWaterArea1 = mapFile.StandingWaterAreas.Areas[1];

            Assert.Equal(2u, standingWaterArea1.UniqueID);
            Assert.Equal("Atlantic Ocean", standingWaterArea1.Name);
            Assert.Equal(string.Empty, standingWaterArea1.LayerName);
            Assert.Equal(0.06f, standingWaterArea1.UvScrollSpeed, 2);
            Assert.Equal(false, standingWaterArea1.UseAdditiveBlending);
            Assert.Equal("WaterRippleBump.tga", standingWaterArea1.BumpMapTexture);
            Assert.Equal("SkyEnv.tga", standingWaterArea1.SkyTexture);
            Assert.Equal(5, standingWaterArea1.Points.Length);
            Assert.Equal(0u, standingWaterArea1.WaterHeight);
            Assert.Equal("WaterCaribbean.w3d", standingWaterArea1.FxShader);
            Assert.Equal("LUTDepthTint.tga", standingWaterArea1.DepthColors);
        }

        [Fact]
        public void BlendTileData_RiverAreas()
        {
            var mapFile = GetMapFile();

            Assert.Equal(2, mapFile.RiverAreas.Areas.Length);

            var riverArea0 = mapFile.RiverAreas.Areas[0];

            Assert.Equal(6u, riverArea0.UniqueID);
            Assert.Equal("Amazon River", riverArea0.Name);
            Assert.Equal(string.Empty, riverArea0.LayerName);
            Assert.Equal(0.07f, riverArea0.UvScrollSpeed, 2);
            Assert.Equal(true, riverArea0.UseAdditiveBlending);
            Assert.Equal("TSWater.tga", riverArea0.RiverTexture);
            Assert.Equal("Noise0001.tga", riverArea0.NoiseTexture);
            Assert.Equal("TLava_Mor02.tga", riverArea0.AlphaEdgeTexture);
            Assert.Equal("WaterSurfaceBubbles01.tga", riverArea0.SparkleTexture);
            Assert.Equal(128, riverArea0.Color.R);
            Assert.Equal(128, riverArea0.Color.G);
            Assert.Equal(64, riverArea0.Color.B);
            Assert.Equal(0.8f, riverArea0.Alpha, 2);
            Assert.Equal(5u, riverArea0.WaterHeight);
            Assert.Equal("Medium", riverArea0.MinimumWaterLod);
            Assert.Equal(4, riverArea0.Lines.Length);

            var riverArea1 = mapFile.RiverAreas.Areas[1];

            Assert.Equal(7u, riverArea1.UniqueID);
            Assert.Equal("Hudson River", riverArea1.Name);
            Assert.Equal(string.Empty, riverArea1.LayerName);
            Assert.Equal(0.06f, riverArea1.UvScrollSpeed, 2);
            Assert.Equal(false, riverArea1.UseAdditiveBlending);
            Assert.Equal("TWWater01.tga", riverArea1.RiverTexture);
            Assert.Equal("Noise0000.tga", riverArea1.NoiseTexture);
            Assert.Equal("TWAlphaEdge.tga", riverArea1.AlphaEdgeTexture);
            Assert.Equal("WaterSurfaceBubbles.tga", riverArea1.SparkleTexture);
            Assert.Equal(255, riverArea1.Color.R);
            Assert.Equal(255, riverArea1.Color.G);
            Assert.Equal(255, riverArea1.Color.B);
            Assert.Equal(1.0f, riverArea1.Alpha, 2);
            Assert.Equal(0u, riverArea1.WaterHeight);
            Assert.Equal(string.Empty, riverArea1.MinimumWaterLod);
            Assert.Equal(2, riverArea1.Lines.Length);
        }

        [Fact]
        public void BlendTileData_StandingWaveAreas()
        {
            var mapFile = GetMapFile();

            Assert.Equal(2, mapFile.StandingWaveAreas.Areas.Length);

            var standingWaveArea0 = mapFile.StandingWaveAreas.Areas[0];

            Assert.Equal(1u, standingWaveArea0.UniqueID);
            Assert.Equal("Wave Area 1", standingWaveArea0.Name);
            Assert.Equal(string.Empty, standingWaveArea0.LayerName);
            Assert.Equal(36u, standingWaveArea0.FinalWidth);
            Assert.Equal(25u, standingWaveArea0.FinalHeight);
            Assert.Equal(56u, standingWaveArea0.InitialWidthFraction);
            Assert.Equal(86u, standingWaveArea0.InitialHeightFraction);
            Assert.Equal(14u, standingWaveArea0.InitialVelocity);
            Assert.Equal(2236u, standingWaveArea0.TimeToFade);
            Assert.Equal(2018u, standingWaveArea0.TimeToCompress);
            Assert.Equal(3709u, standingWaveArea0.TimeOffset2ndWave);
            Assert.Equal(49u, standingWaveArea0.DistanceFromShore);
            Assert.Equal("wave256.tga", standingWaveArea0.Texture);
            Assert.Equal(true, standingWaveArea0.EnablePcaWave);

            var standingWaveArea1 = mapFile.StandingWaveAreas.Areas[1];

            Assert.Equal(2u, standingWaveArea1.UniqueID);
            Assert.Equal("Wave Area 2", standingWaveArea1.Name);
            Assert.Equal(string.Empty, standingWaveArea1.LayerName);
            Assert.Equal(28u, standingWaveArea1.FinalWidth);
            Assert.Equal(18u, standingWaveArea1.FinalHeight);
            Assert.Equal(50u, standingWaveArea1.InitialWidthFraction);
            Assert.Equal(100u, standingWaveArea1.InitialHeightFraction);
            Assert.Equal(5u, standingWaveArea1.InitialVelocity);
            Assert.Equal(2000u, standingWaveArea1.TimeToFade);
            Assert.Equal(1000u, standingWaveArea1.TimeToCompress);
            Assert.Equal(3000u, standingWaveArea1.TimeOffset2ndWave);
            Assert.Equal(40u, standingWaveArea1.DistanceFromShore);
            Assert.Equal("wave256.tga", standingWaveArea1.Texture);
            Assert.Equal(false, standingWaveArea1.EnablePcaWave);
        }

        private static MapFile GetMapFile([CallerMemberName] string testName = null)
        {
            var fileName = Path.Combine("Map", "Assets", testName + ".map");

            using (var entryStream = File.OpenRead(fileName))
            {
                return MapFile.FromStream(entryStream);
            }
        }
    }
}
