﻿using FluentAssertions;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Actuators.Tests
{
    [TestClass]
    public class TriggerTests
    {
        [TestMethod]
        public void Trigger_Invoke()
        {
            bool attachTriggered = false;
            bool eventTriggered = false;

            var trigger = new Trigger();
            trigger.Attach(() => attachTriggered = true);
            trigger.IsAnyAttached.ShouldBeEquivalentTo(true);

            trigger.Triggered += (s, e) => eventTriggered = true;
            trigger.IsAnyAttached.ShouldBeEquivalentTo(true);

            trigger.Execute();

            attachTriggered.ShouldBeEquivalentTo(true);
            eventTriggered.ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void SensorValueReached_Trigger()
        {
            var sensor = new TestSensor(ActuatorIdFactory.EmptyId);
            var trigger = new SensorValueReachedTrigger(sensor);
            trigger.Target = 10.2F;
            trigger.Delta = 3.0F;

            int triggerCount = 0;
            trigger.Attach(() => triggerCount++);

            sensor.SetValue(5);
            triggerCount.ShouldBeEquivalentTo(0);

            sensor.SetValue(10);
            triggerCount.ShouldBeEquivalentTo(0);

            sensor.SetValue(10.2F);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.SetValue(9.0F);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.SetValue(13.0F);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.SetValue(5.0F);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.SetValue(10.2F);
            triggerCount.ShouldBeEquivalentTo(2);
        }

        [TestMethod]
        public void SensorValueUnderran_Trigger()
        {
            var sensor = new TestSensor(ActuatorIdFactory.EmptyId);
            var trigger = new SensorValueUnderranTrigger(sensor);
            trigger.Target = 10F;
            trigger.Delta = 3F;

            int triggerCount = 0;
            trigger.Attach(() => triggerCount++);

            sensor.SetValue(5);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.SetValue(10);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.SetValue(13.1F);
            triggerCount.ShouldBeEquivalentTo(1);

            sensor.SetValue(9F);
            triggerCount.ShouldBeEquivalentTo(2);

            sensor.SetValue(13.0F);
            triggerCount.ShouldBeEquivalentTo(2);

            sensor.SetValue(5F);
            triggerCount.ShouldBeEquivalentTo(2);

            sensor.SetValue(13.1F);
            triggerCount.ShouldBeEquivalentTo(2);

            sensor.SetValue(9.9F);
            triggerCount.ShouldBeEquivalentTo(3);
        }

        [TestMethod]
        public void Associate_TriggerWithActuatorAction()
        {
            var testButton = new TestButton();
            var testOutput = new TestBinaryStateOutputActuator();

            testButton.GetPressedShortlyTrigger().OnTriggered(testOutput.GetToggleStateAction());

            testOutput.GetState().ShouldBeEquivalentTo(BinaryActuatorState.Off);
            testButton.PressShort();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryActuatorState.On);
            testButton.PressShort();
            testOutput.GetState().ShouldBeEquivalentTo(BinaryActuatorState.Off);
        }
    }
}