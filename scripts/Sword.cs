using Godot;
using System;

public partial class Sword : Node3D
{
	// Called when the node enters the scene tree for the first time.

	private CollisionShape3D HurtBox { get; set; }

    public override void _Ready()
	{
		HurtBox = GetNode<CollisionShape3D>("HurtBox/CollisionShape3D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public async void _OnArea3dBodyEntered(Area3D body) 
	{
		if (body.IsInGroup("destructable"))
		{
			AnimationPlayer destructableAnimationPlayer = body.Owner.GetNode<AnimationPlayer>("AnimationPlayer");
			destructableAnimationPlayer.Play("explode");
			await ToSignal(destructableAnimationPlayer, "animation_finished");
			body.Owner.QueueFree();
		}
	}

	public void SetHurtboxDisabled(bool status)
	{
		HurtBox.Disabled = status;
	}
}
