using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 5.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	private Node3D Neck { get; set; }
	private Node3D Camera { get; set; }
	private Node3D Hand { get; set; }
	private Weapon Weapon { get; set; } 

	private AnimationPlayer AnimationPlayer { get; set; }

    public override void _Ready()
    {
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		Neck = GetNode<Node3D>("Neck");
		Camera = GetNode<Node3D>("Neck/Camera3D");
		Hand = Camera.GetNode<Node3D>("Hand");
		// TODO : Handle which weapon is equipped separately
		Weapon = Hand.GetChild<Weapon>(0);
    }

    public override async void _Process(double delta)
    {
        if (Input.IsActionJustPressed("attack"))
		{
			Weapon.SetHurtboxDisabled(false);
			GD.Print("Animation Name: " + Weapon.GetAnimationName());
            AnimationPlayer.Play(Weapon.GetAnimationName());
			await ToSignal(AnimationPlayer, "animation_finished");
            Weapon.SetHurtboxDisabled(true);
        }
    }

    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = -direction.X * Speed;
			velocity.Z = -direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();

		if (GetSlideCollisionCount() > 1)
		{
            for (int i = 0; i < GetSlideCollisionCount(); i++)
            {
                KinematicCollision3D c = GetSlideCollision(i);
                if (c.GetCollider() is RigidBody3D)
                {
                    ((RigidBody3D)c.GetCollider()).ApplyCentralImpulse(-c.GetNormal());
                }
            }
        }
	}

	const float RotationConstant = 0.005f;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton)
			Input.MouseMode = Input.MouseModeEnum.Captured;
		if (@event.IsActionPressed("ui_cancel"))
			Input.MouseMode = Input.MouseModeEnum.Visible;

		if (Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			if (@event is InputEventMouseMotion eventMouseMotion)
			{
				RotateY(-eventMouseMotion.Relative.X * RotationConstant);
				Camera.RotateX(eventMouseMotion.Relative.Y * RotationConstant);
				Camera.Rotation = new Vector3(Mathf.Clamp(Camera.Rotation.X, -0.75f, 1f), Camera.Rotation.Y, Camera.Rotation.Z); // Radians
			}
		}
	}
}
