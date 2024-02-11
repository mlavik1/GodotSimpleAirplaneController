using Godot;
using System;

public partial class CameraFollow : Node3D
{
	[Export]
	public Node3D followTarget;

	private float positionLerpSpeed = 5.0f;
	private float rotationLerpSpeed = 3.0f;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		this.GlobalPosition = this.GlobalPosition.Lerp(followTarget.GlobalPosition, (float)delta * positionLerpSpeed);
		Quaternion quat = this.GlobalBasis.GetRotationQuaternion().Slerp(followTarget.GlobalBasis.GetRotationQuaternion(), (float)delta * rotationLerpSpeed);
		this.GlobalBasis = new Basis(quat);
	}
}
