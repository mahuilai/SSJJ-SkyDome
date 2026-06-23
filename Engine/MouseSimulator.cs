using System;
using System.Collections.Generic;
using Assets.Scripts.Input;
using SkyDome.Cfg;
using UnityEngine;
namespace SkyDome.Engine
{
	public class InputDriver : IDeviceInput
	{

        public bool GetKeyDown(KeyCode keyCode)
		{
			InputDriver.InputState inputState;
			if (this.TryGetKeyState(keyCode, out inputState))
			{
				return inputState == (InputDriver.InputState)2;
			}
			return Input.GetKeyDown(keyCode);
		}
		public bool AnyKey()
		{
			try
			{
				Action preInputCallback = InputDriver.PreInputCallback;
				if (preInputCallback != null)
				{
					preInputCallback();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Format("PreInputCallback 错误: {0}", ex));
			}
			if (!this.HasActiveKeyInput() && !this.HasActiveMouseInput())
			{
				return Input.anyKey;
			}
			return true;
		}
		private bool HasActiveKeyInput()
		{
			foreach (InputDriver.InputState inputState in InputDriver._forcedKeys.Values)
			{
				if (inputState != InputDriver.InputState.None && inputState != (InputDriver.InputState)3)
				{
					return true;
				}
			}
			return false;
		}
		public static void ForceKey(KeyCode keyCode, InputDriver.InputState state)
		{
			InputDriver._forcedKeys[keyCode] = state;
		}
		public float GetAxis(string axis)
		{
			float axis2 = Input.GetAxis(axis);
			if (axis == "Mouse X")
			{
				return axis2 + InputDriver.ConsumeAxisDelta(ref InputDriver.ForceAxisDelta.x);
			}
			if (axis == "Mouse Y")
			{
				return axis2 + InputDriver.ConsumeAxisDelta(ref InputDriver.ForceAxisDelta.y);
			}
			return axis2;
		}

        public bool GetKey(KeyCode keyCode)
		{
			InputDriver.InputState inputState;
			if (this.TryGetKeyState(keyCode, out inputState))
			{
				return this.ProcessKeyState(keyCode, inputState);
			}
			return Input.GetKey(keyCode);
		}
		public static void ForceMouseButton(int mouseButton, InputDriver.InputState state)
		{
			InputDriver._forcedMouseButtons[mouseButton] = state;
		}

        private bool ProcessKeyState(KeyCode keyCode, InputDriver.InputState state)
		{
			switch (state)
			{
			case InputDriver.InputState.TrueKeep:
				return true;
			case InputDriver.InputState.TrueOnce:
				InputDriver._forcedKeys[keyCode] = InputDriver.InputState.None;
				return true;
			case InputDriver.InputState.FalseKeep:
				return false;
			case InputDriver.InputState.FalseOnce:
				InputDriver._forcedKeys[keyCode] = InputDriver.InputState.None;
				return false;
			default:
				return Input.GetKey(keyCode);
			}
		}
		private bool HasKeyDownInput()
		{
			using (Dictionary<KeyCode, InputDriver.InputState>.ValueCollection.Enumerator enumerator = InputDriver._forcedKeys.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == (InputDriver.InputState)2)
					{
						return true;
					}
				}
			}
			return false;
		}
		public static event Action PreInputCallback;
		public bool GetMouseButtonUp(int button)
		{
			return Input.GetMouseButtonUp(button);
		}

        private bool HasActiveMouseInput()
		{
			foreach (InputDriver.InputState inputState in InputDriver._forcedMouseButtons.Values)
			{
				if (inputState != InputDriver.InputState.None && inputState != (InputDriver.InputState)3)
				{
					return true;
				}
			}
			return false;
		}
		public bool GetMouseButton(int button)
		{
			InputDriver.InputState inputState;
			if (this.TryGetMouseState(button, out inputState))
			{
				return this.ProcessMouseState(button, inputState);
			}
			if (SettingsStore.AntiMouse1 &&
				Contexts.sharedInstance != null &&
				Contexts.sharedInstance.player != null &&
				Contexts.sharedInstance.player.myPlayerEntity != null &&
				Contexts.sharedInstance.player.myPlayerEntity.currentWeapon != null &&
				Contexts.sharedInstance.player.myPlayerEntity.currentWeapon.Weapon < 3 &&
				Contexts.sharedInstance.weapon != null &&
				Contexts.sharedInstance.weapon.currentWeaponEntity != null &&
				Contexts.sharedInstance.weapon.currentWeaponEntity.basicInfo != null &&
				Contexts.sharedInstance.weapon.currentWeaponEntity.basicInfo.Info != null &&
				Contexts.sharedInstance.weapon.currentWeaponEntity.basicInfo.Info.WeaponType != 5 &&
				button == 1)
			{
				return button == 0;
			}
			return Input.GetMouseButton(button);
		}

        private bool TryGetMouseState(int button, out InputDriver.InputState state)
		{
			return InputDriver._forcedMouseButtons.TryGetValue(button, out state) && state > InputDriver.InputState.None;
		}
		public bool GetMouseButtonDown(int button)
		{
			return Input.GetMouseButtonDown(button);
		}
		private bool TryGetKeyState(KeyCode keyCode, out InputDriver.InputState state)
		{
			return InputDriver._forcedKeys.TryGetValue(keyCode, out state) && state > InputDriver.InputState.None;
		}
		public bool AnyKeyDown()
		{
			if (!this.HasKeyDownInput())
			{
				return Input.anyKeyDown;
			}
			return true;
		}
		private static float ConsumeAxisDelta(ref float axisValue)
		{
			float num = axisValue;
			axisValue = 0f;
			return num;
		}

        private bool ProcessMouseState(int button, InputDriver.InputState state)
		{
			switch (state)
			{
			case InputDriver.InputState.TrueKeep:
				return true;
			case InputDriver.InputState.TrueOnce:
				InputDriver._forcedMouseButtons[button] = InputDriver.InputState.None;
				return true;
			case InputDriver.InputState.FalseKeep:
				return false;
			case InputDriver.InputState.FalseOnce:
				InputDriver._forcedMouseButtons[button] = InputDriver.InputState.None;
				return false;
			default:
				return Input.GetMouseButton(button);
			}
		}
		private static readonly Dictionary<KeyCode, InputDriver.InputState> _forcedKeys = new Dictionary<KeyCode, InputDriver.InputState>();
		private static readonly Dictionary<int, InputDriver.InputState> _forcedMouseButtons = new Dictionary<int, InputDriver.InputState>();
		public static Vector2 ForceAxisPersistent = Vector2.zero;
		public static Vector2 ForceAxisDelta = Vector2.zero;
		public enum InputState
		{
			None,
			TrueKeep,
			TrueOnce,
			FalseKeep,
			FalseOnce
		}
	}
}