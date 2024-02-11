using Godot;
using System;

public partial class PlayerAircraftController : Node3D
{
	[Export]
	public Aircraft aircraft;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		if (aircraft == null)
			return;

		// Input: Throttle
		float throttle = 0.0f;
		if (Input.IsKeyPressed(Key.W))
			throttle = 1.0f;
		else
			throttle = 0.0f;
		aircraft.SetInputThrottle(throttle);

		Vector2 axis = Vector2.Zero;
		// Roll
		if (Input.IsKeyPressed(Key.Left))
			axis.X = -1.0f;
		else if (Input.IsKeyPressed(Key.Right))
			axis.X = 1.0f;
		else
			axis.X = 0.0f;

		// Pitch
		if (Input.IsKeyPressed(Key.Down))
			axis.Y = 1.0f;
		else if (Input.IsKeyPressed(Key.Up))
			axis.Y = -1.0f;
		else
			axis.Y = 0.0f;

		aircraft.SetInputAxis(axis);
	}
}
