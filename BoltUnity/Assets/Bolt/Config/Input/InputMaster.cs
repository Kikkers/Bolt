// GENERATED AUTOMATICALLY FROM 'Assets/Bolt/Settings/Input/Inputs.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @InputMaster : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputMaster()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Inputs"",
    ""maps"": [
        {
            ""name"": ""Character"",
            ""id"": ""54e38c18-401d-4483-a9ab-007d3ca007a7"",
            ""actions"": [
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""a9e39fe1-0053-4c57-840b-b52f18a07743"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Crouch"",
                    ""type"": ""Button"",
                    ""id"": ""03dfe1d8-c90d-4735-952e-fd351e5fcbc8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Direction"",
                    ""type"": ""Value"",
                    ""id"": ""9ad5ceaf-51e1-43ea-b55c-88343cab7ab7"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Aim"",
                    ""type"": ""Value"",
                    ""id"": ""1abca4fc-87fd-4233-b87a-c77ad193b1aa"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""AltMode"",
                    ""type"": ""Button"",
                    ""id"": ""5247c1dc-ac21-490c-8714-b34777c912f2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""03d5eea3-1071-4b4a-a001-9095643f91f4"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""63abe082-0769-4241-9f20-df863d4752da"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""0f42f477-923c-4e9b-8066-18e861b403e1"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Direction"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""5603c31d-b8e9-4314-9bf2-fb03389d984d"",
                    ""path"": ""<Keyboard>/#(W)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Direction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""6f41b845-e819-439d-9023-73d3737cedae"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Direction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""d3177040-68f3-4b5c-a9c1-f62275bb1f1d"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Direction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""23264051-6286-4f39-8fd9-4608feadffe9"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Direction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""969726f2-c05b-47e0-a404-310e4f4da3fc"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": ""ScaleVector2(x=0.4,y=0.4)"",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""Aim"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1d00fee3-d168-4f29-9ad1-1a0ec82a0b12"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": ""KeyboardMouse"",
                    ""action"": ""AltMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""KeyboardMouse"",
            ""bindingGroup"": ""KeyboardMouse"",
            ""devices"": []
        }
    ]
}");
        // Character
        m_Character = asset.FindActionMap("Character", throwIfNotFound: true);
        m_Character_Jump = m_Character.FindAction("Jump", throwIfNotFound: true);
        m_Character_Crouch = m_Character.FindAction("Crouch", throwIfNotFound: true);
        m_Character_Direction = m_Character.FindAction("Direction", throwIfNotFound: true);
        m_Character_Aim = m_Character.FindAction("Aim", throwIfNotFound: true);
        m_Character_AltMode = m_Character.FindAction("AltMode", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Character
    private readonly InputActionMap m_Character;
    private ICharacterActions m_CharacterActionsCallbackInterface;
    private readonly InputAction m_Character_Jump;
    private readonly InputAction m_Character_Crouch;
    private readonly InputAction m_Character_Direction;
    private readonly InputAction m_Character_Aim;
    private readonly InputAction m_Character_AltMode;
    public struct CharacterActions
    {
        private @InputMaster m_Wrapper;
        public CharacterActions(@InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @Jump => m_Wrapper.m_Character_Jump;
        public InputAction @Crouch => m_Wrapper.m_Character_Crouch;
        public InputAction @Direction => m_Wrapper.m_Character_Direction;
        public InputAction @Aim => m_Wrapper.m_Character_Aim;
        public InputAction @AltMode => m_Wrapper.m_Character_AltMode;
        public InputActionMap Get() { return m_Wrapper.m_Character; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CharacterActions set) { return set.Get(); }
        public void SetCallbacks(ICharacterActions instance)
        {
            if (m_Wrapper.m_CharacterActionsCallbackInterface != null)
            {
                @Jump.started -= m_Wrapper.m_CharacterActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_CharacterActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_CharacterActionsCallbackInterface.OnJump;
                @Crouch.started -= m_Wrapper.m_CharacterActionsCallbackInterface.OnCrouch;
                @Crouch.performed -= m_Wrapper.m_CharacterActionsCallbackInterface.OnCrouch;
                @Crouch.canceled -= m_Wrapper.m_CharacterActionsCallbackInterface.OnCrouch;
                @Direction.started -= m_Wrapper.m_CharacterActionsCallbackInterface.OnDirection;
                @Direction.performed -= m_Wrapper.m_CharacterActionsCallbackInterface.OnDirection;
                @Direction.canceled -= m_Wrapper.m_CharacterActionsCallbackInterface.OnDirection;
                @Aim.started -= m_Wrapper.m_CharacterActionsCallbackInterface.OnAim;
                @Aim.performed -= m_Wrapper.m_CharacterActionsCallbackInterface.OnAim;
                @Aim.canceled -= m_Wrapper.m_CharacterActionsCallbackInterface.OnAim;
                @AltMode.started -= m_Wrapper.m_CharacterActionsCallbackInterface.OnAltMode;
                @AltMode.performed -= m_Wrapper.m_CharacterActionsCallbackInterface.OnAltMode;
                @AltMode.canceled -= m_Wrapper.m_CharacterActionsCallbackInterface.OnAltMode;
            }
            m_Wrapper.m_CharacterActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Crouch.started += instance.OnCrouch;
                @Crouch.performed += instance.OnCrouch;
                @Crouch.canceled += instance.OnCrouch;
                @Direction.started += instance.OnDirection;
                @Direction.performed += instance.OnDirection;
                @Direction.canceled += instance.OnDirection;
                @Aim.started += instance.OnAim;
                @Aim.performed += instance.OnAim;
                @Aim.canceled += instance.OnAim;
                @AltMode.started += instance.OnAltMode;
                @AltMode.performed += instance.OnAltMode;
                @AltMode.canceled += instance.OnAltMode;
            }
        }
    }
    public CharacterActions @Character => new CharacterActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("KeyboardMouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    public interface ICharacterActions
    {
        void OnJump(InputAction.CallbackContext context);
        void OnCrouch(InputAction.CallbackContext context);
        void OnDirection(InputAction.CallbackContext context);
        void OnAim(InputAction.CallbackContext context);
        void OnAltMode(InputAction.CallbackContext context);
    }
}
