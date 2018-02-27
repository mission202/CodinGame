using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CodinGame.Tests
{
    public class Tests
    {
        [Fact]
        public void TestsWork()
        {
            Assert.True(true);
        }

        [Fact]
        public void ParseSampleData()
        {
            var sample = new SampleDataFile("0 -1 1 0 1 0 2 1 0 0");
            Assert.Equal(0, sample.Id);
            Assert.Equal(-1, sample.CarriedBy);
            Assert.True(sample.InCloud);
            Assert.Equal(1, sample.Rank);
            Assert.Equal("0", sample.Gain);
            Assert.Equal(1, sample.Health);
            Assert.Equal(0, sample.Cost.A);
            Assert.Equal(2, sample.Cost.B);
            Assert.Equal(1, sample.Cost.C);
            Assert.Equal(0, sample.Cost.D);
            Assert.Equal(0, sample.Cost.E);
        }

        [Fact]
        public void ShouldGet3Rank1SampplesAtStart()
        {
            var game = new Game("START_POS 0 0 0 0 0 0 0 0 0 0 0 0|START_POS 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|0|//||");
            Assert.Equal(Goto.Samples, game.GetNextAction());
        }

        [Fact]
        public void ShouldWaitWhileTravelling()
        {
            // START_POS -> SAMPLES = 1 Turn
            var game = new Game("SAMPLES 1 0 0 0 0 0 0 0 0 0 0 0|SAMPLES 1 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|0|//||WAIT");
            Assert.Equal(Actions.Wait, game.GetNextAction());
        }

        [Fact]
        public void ShouldGoToDiagnosisIfHave3Samples()
        {
            var game = new Game("SAMPLES 0 0 0 0 0 0 0 0 0 0 0 0|SAMPLES 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 0 -1 -1 -1 -1 -1 -1|2 0 1 0 -1 -1 -1 -1 -1 -1|4 0 1 0 -1 -1 -1 -1 -1 -1|1 1 1 0 -1 -1 -1 -1 -1 -1|3 1 1 0 -1 -1 -1 -1 -1 -1|5 1 1 0 -1 -1 -1 -1 -1 -1//||");
            Assert.Equal(Goto.Diagnosis, game.GetNextAction());
        }

        [Fact]
        public void ShouldDiagnoseUndiagnosedSamplesAndGotoMolecules()
        {
            var game = new Game("DIAGNOSIS 0 0 0 0 0 0 0 0 0 0 0 0|DIAGNOSIS 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 0 -1 -1 -1 -1 -1 -1|2 0 1 0 -1 -1 -1 -1 -1 -1|4 0 1 0 -1 -1 -1 -1 -1 -1|1 1 1 0 -1 -1 -1 -1 -1 -1|3 1 1 0 -1 -1 -1 -1 -1 -1|5 1 1 0 -1 -1 -1 -1 -1 -1//||");
            Assert.Equal(Actions.Connect(0), game.GetNextAction());
            Assert.Equal(Actions.Connect(2), game.GetNextAction());
            Assert.Equal(Actions.Connect(4), game.GetNextAction());
            Assert.Equal(Goto.Molecules, game.GetNextAction());
        }

        [Fact]
        public void ShouldGetMostValuableAvailableMolecule()
        {
            // 0: AACEE (1)
            // 2: AABCE (1)
            // 4: EEEE (10)
            // Available: AAAAABBBBBCCCCCDDDDDEEEEE

            var game = new Game("MOLECULES 0 0 0 0 0 0 0 0 0 0 0 0|MOLECULES 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 D 1 2 0 1 0 2|2 0 1 D 1 2 1 1 0 1|4 0 1 C 10 0 0 0 0 4|1 1 1 D 1 1 0 0 1 3|3 1 1 B 1 0 0 0 0 3|5 1 1 A 1 0 2 2 0 1//0,2,4||");
            Assert.Equal(Actions.Connect('E'), game.GetNextAction());
        }

        // TODO: Fixup 'Researched' Tracking
        // TODO: State needs to be much shorter :) Get's truncated by CG
        [Fact]
        public void ResearchesMostValuableAvailableSample()
        {
            //var game = new Game("START_POS 0 0 0 0 0 0 0 0 0 0 0 0|START_POS 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|0|/SAMPLES 1 0 0 0 0 0 0 0 0 0 0 0|SAMPLES 1 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|0|/SAMPLES 0 0 0 0 0 0 0 0 0 0 0 0|SAMPLES 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|0|/SAMPLES 0 0 0 0 0 0 0 0 0 0 0 0|SAMPLES 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|2|0 0 1 0 -1 -1 -1 -1 -1 -1,1 1 1 0 -1 -1 -1 -1 -1 -1/SAMPLES 0 0 0 0 0 0 0 0 0 0 0 0|SAMPLES 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|4|0 0 1 0 -1 -1 -1 -1 -1 -1,2 0 1 0 -1 -1 -1 -1 -1 -1,1 1 1 0 -1 -1 -1 -1 -1 -1,3 1 1 0 -1 -1 -1 -1 -1 -1/SAMPLES 0 0 0 0 0 0 0 0 0 0 0 0|SAMPLES 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 0 -1 -1 -1 -1 -1 -1,2 0 1 0 -1 -1 -1 -1 -1 -1,4 0 1 0 -1 -1 -1 -1 -1 -1,1 1 1 0 -1 -1 -1 -1 -1 -1,3 1 1 0 -1 -1 -1 -1 -1 -1,5 1 1 0 -1 -1 -1 -1 -1 -1/DIAGNOSIS 2 0 0 0 0 0 0 0 0 0 0 0|DIAGNOSIS 2 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 0 -1 -1 -1 -1 -1 -1,2 0 1 0 -1 -1 -1 -1 -1 -1,4 0 1 0 -1 -1 -1 -1 -1 -1,1 1 1 0 -1 -1 -1 -1 -1 -1,3 1 1 0 -1 -1 -1 -1 -1 -1,5 1 1 0 -1 -1 -1 -1 -1 -1/DIAGNOSIS 1 0 0 0 0 0 0 0 0 0 0 0|DIAGNOSIS 1 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 0 -1 -1 -1 -1 -1 -1,2 0 1 0 -1 -1 -1 -1 -1 -1,4 0 1 0 -1 -1 -1 -1 -1 -1,1 1 1 0 -1 -1 -1 -1 -1 -1,3 1 1 0 -1 -1 -1 -1 -1 -1,5 1 1 0 -1 -1 -1 -1 -1 -1/DIAGNOSIS 0 0 0 0 0 0 0 0 0 0 0 0|DIAGNOSIS 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 0 -1 -1 -1 -1 -1 -1,2 0 1 0 -1 -1 -1 -1 -1 -1,4 0 1 0 -1 -1 -1 -1 -1 -1,1 1 1 0 -1 -1 -1 -1 -1 -1,3 1 1 0 -1 -1 -1 -1 -1 -1,5 1 1 0 -1 -1 -1 -1 -1 -1/DIAGNOSIS 0 0 0 0 0 0 0 0 0 0 0 0|DIAGNOSIS 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 D 1 3 0 0 0 0,2 0 1 0 -1 -1 -1 -1 -1 -1,4 0 1 0 -1 -1 -1 -1 -1 -1,1 1 1 A 1 0 3 0 0 0,3 1 1 0 -1 -1 -1 -1 -1 -1,5 1 1 0 -1 -1 -1 -1 -1 -1/DIAGNOSIS 0 0 0 0 0 0 0 0 0 0 0 0|DIAGNOSIS 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 D 1 3 0 0 0 0,2 0 1 B 1 1 0 1 2 1,4 0 1 0 -1 -1 -1 -1 -1 -1,1 1 1 A 1 0 3 0 0 0,3 1 1 E 1 0 0 3 0 0,5 1 1 0 -1 -1 -1 -1 -1 -1/DIAGNOSIS 0 0 0 0 0 0 0 0 0 0 0 0|DIAGNOSIS 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 D 1 3 0 0 0 0,2 0 1 B 1 1 0 1 2 1,4 0 1 E 10 0 4 0 0 0,1 1 1 A 1 0 3 0 0 0,3 1 1 E 1 0 0 3 0 0,5 1 1 B 1 0 0 2 0 2/MOLECULES 2 0 0 0 0 0 0 0 0 0 0 0|MOLECULES 2 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 D 1 3 0 0 0 0,2 0 1 B 1 1 0 1 2 1,4 0 1 E 10 0 4 0 0 0,1 1 1 A 1 0 3 0 0 0,3 1 1 E 1 0 0 3 0 0,5 1 1 B 1 0 0 2 0 2/MOLECULES 1 0 0 0 0 0 0 0 0 0 0 0|MOLECULES 1 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 D 1 3 0 0 0 0,2 0 1 B 1 1 0 1 2 1,4 0 1 E 10 0 4 0 0 0,1 1 1 A 1 0 3 0 0 0,3 1 1 E 1 0 0 3 0 0,5 1 1 B 1 0 0 2 0 2/MOLECULES 0 0 0 0 0 0 0 0 0 0 0 0|MOLECULES 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|6|0 0 1 D 1 3 0 0 0 0,2 0 1 B 1 1 0 1 2 1,4 0 1 E 10 0 4 0 0 0,1 1 1 A 1 0 3 0 0 0,3 1 1 E 1 0 0 3 0 0,5 1 1 B 1 0 0 2 0 2/MOLECULES 0 0 0 1 0 0 0 0 0 0 0 0|MOLECULES 0 0 0 1 0 0 0 0 0 0 0 0|5 3 5 5 5|6|0 0 1 D 1 3 0 0 0 0,2 0 1 B 1 1 0 1 2 1,4 0 1 E 10 0 4 0 0 0,1 1 1 A 1 0 3 0 0 0,3 1 1 E 1 0 0 3 0 0,5 1 1 B 1 0 0 2 0 2/MOLECULES 0 0 0 2 0 0 0 0 0 0 0 0|MOLECULES 0 0 0 2 0 0 0 0 0 0 0 0|5 1 5 5 5|6|0 0 1 D 1 3 0 0 0 0,2 0 1 B 1 1 0 1 2 1,4 0 1 E 10 0 4 0 0 0,1 1 1 A 1 0 3 0 0 0,3 1 1 E 1 0 0 3 0 0,5 1 1 B 1 0 0 2 0 2/MOLECULES 0 0 1 2 0 0 0 0 0 0 0 0|MOLECULES 0 0 0 3 0 0 0 0 0 0 0 0|4 0 5 5 5|6|0 0 1 D 1 3 0 0 0 0,2 0 1 B 1 1 0 1 2 1,4 0 1 E 10 0 4 0 0 0,1 1 1 A 1 0 3 0 0 0,3 1 1 E 1 0 0 3 0 0,5 1 1 B 1 0 0 2 0 2/MOLECULES 0 0 2 2 0 0 0 0 0 0 0 0|LABORATORY 2 0 0 3 0 0 0 0 0 0 0 0|3 0 5 5 5|6|0 0 1 D 1 3 0 0 0 0,2 0 1 B 1 1 0 1 2 1,4 0 1 E 10 0 4 0 0 0,1 1 1 A 1 0 3 0 0 0,3 1 1 E 1 0 0 3 0 0,5 1 1 B 1 0 0 2 0 2/MOLECULES 0 0 3 2 0 0 0 0 0 0 0 0|LABORATORY 1 0 0 3 0 0 0 0 0 0 0 0|2 0 5 5 5|6|0 0 1 D 1 3 0 0 0 0,2 0 1 B 1 1 0 1 2 1,4 0 1 E 10 0 4 0 0 0,1 1 1 A 1 0 3 0 0 0,3 1 1 E 1 0 0 3 0 0,5 1 1 B 1 0 0 2 0 2/LABORATORY 2 0 3 2 0 0 0 0 0 0 0 0|LABORATORY 0 0 0 3 0 0 0 0 0 0 0 0|2 0 5 5 5|6|0 0 1 D 1 3 0 0 0 0,2 0 1 B 1 1 0 1 2 1,4 0 1 E 10 0 4 0 0 0,1 1 1 A 1 0 3 0 0 0...")
        }

        [Fact]
        public void CanSerialiseState()
        {
            // Player, Opponent and Availability is Available Turn-by-Turn
            // Diagnosed, Research Tracking and Queue is Internal
            var game = new Game();
            game.Setup("START_POS 0 0 0 0 0 0 0 0 0 0 0 0|START_POS 0 0 0 0 0 0 0 0 0 0 0 0|5 5 5 5 5|0|");
            var serialised = game.Serialise;
            var deserialised = new Game(serialised);
            Assert.Equal(Modules.START_POS, deserialised.Player.Target);
        }
    }
}
