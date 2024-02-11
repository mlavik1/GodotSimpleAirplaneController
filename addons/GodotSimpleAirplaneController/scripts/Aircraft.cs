using Godot;
using System;

public partial class Aircraft : Node3D
{
	public enum RotationMode
	{
		Persistent,
		ResetOnZero
	}

	[Export]
	public AudioStreamPlayer3D engineStreamPlayer;

	private RotationMode rotationModeRoll = RotationMode.ResetOnZero;
	private RotationMode rotationModePitch = RotationMode.Persistent;

	[Export]
	private float maxSpeed = 0.8f;
	[Export]
	private float acceleration = 0.25f;
	[Export]
	private float deceleration = 0.05f;
	[Export]
	private float maxRoll = 0.78f;
	[Export]
	private float rollAcceleration = 1.5f;
	[Export]
	private float maxPitch = 1.5f;
	[Export]
	private float pitchAcceleration = 1.3f;
	[Export]
	private float yawAcceleration = 1.0f;
	[Export]
	private float noseDropSpeed = 1.1f;
	[Export]
	private float noseDropTarget = -0.2f;
	[Export]
	private float gravity = 0.3f;
	[Export]
	private float stallPitchSpeed = 1.0f;

	private float speed = 0.0f;
	private float roll = 0.0f;
	private float pitch = 0.0f;
	private float yaw = 0.0f;

	private Vector3 startPosition;
	private bool isOnGround = true;

	private Vector2 inputAxis = Vector2.Zero;
	private float inputThrottle = 0.0f;

	public void SetInputAxis(Vector2 axis)
	{
		inputAxis = axis;
	}

	public void SetInputThrottle(float throttle)
	{
		inputThrottle = throttle;
	}

	public override void _Ready()
	{
		startPosition = Position;
	}

	public override void _Process(double delta)
	{
		if (Position.Y > startPosition.Y)
		{
			isOnGround = false;
		}

		// Speed
		float targetSpeed = maxSpeed * inputThrottle;
		if (speed < targetSpeed)
			speed = Mathf.Min(speed + (float)delta * acceleration, targetSpeed);
		else if(speed > targetSpeed)
			speed = Mathf.Max(speed - (float)delta * deceleration, targetSpeed);

		// Roll
		if (rotationModeRoll == RotationMode.ResetOnZero)
		{
			float targetRoll = maxRoll * -inputAxis.X;
			if (targetRoll > roll)
				roll = Mathf.Min(roll + (float)delta * rollAcceleration, targetRoll);
			else if(targetRoll < roll)
				roll = Mathf.Max(roll - (float)delta * rollAcceleration, targetRoll);
		}
		else
		{
			float rollAxis = -inputAxis.X;
			roll = Mathf.Clamp(roll + (float)delta * rollAcceleration * -inputAxis.X, -maxRoll, maxRoll);
		}

		float tRoll = Mathf.InverseLerp(0.0f, Mathf.DegToRad(90.0f), Mathf.Abs(roll));
		if (rotationModePitch == RotationMode.ResetOnZero)
		{
			float targetPitch = maxPitch * inputAxis.Y;
			if (pitch < targetPitch)
			{
				yaw += tRoll * yawAcceleration * (float)delta * Mathf.Sign(roll) * inputAxis.Y;
				pitch = Mathf.Min(pitch + (float)delta * pitchAcceleration * (1.0f - tRoll), targetPitch);
			}
			else if (pitch > targetPitch)
			{
				yaw += tRoll * yawAcceleration * (float)delta * Mathf.Sign(roll) * inputAxis.Y;
				pitch = Mathf.Max(pitch - (float)delta * pitchAcceleration * (1.0f - tRoll), targetPitch);
			}
		}
		else
		{
			yaw += tRoll * yawAcceleration * (float)delta * Mathf.Sign(roll) * inputAxis.Y;
			pitch = Mathf.Clamp(pitch + (float)delta * pitchAcceleration * inputAxis.Y * (1.0f - tRoll), -maxPitch, maxPitch);
		}

		// Pitch => yaw
		yaw += (roll / maxRoll) * yawAcceleration * (float)delta;
		
		// Point nose down when rolling
		if (Mathf.Abs(roll) > 0.4f && pitch > noseDropTarget)
		{
			float t1 = Mathf.InverseLerp(0.4f, maxRoll, Mathf.Abs(roll));
			float t2 = Mathf.InverseLerp(noseDropTarget, maxPitch, Mathf.Abs(pitch));
			pitch = Mathf.Max(noseDropTarget, pitch - noseDropSpeed * (float)delta * t1 * t2);
		}

		// Gravity + stall
		if (!isOnGround && speed < maxSpeed)
		{
			float tSpeed = speed / maxSpeed;
			this.Position += Vector3.Down * gravity * (float)delta * (1.0f - tSpeed);
			pitch = Mathf.Max(Mathf.DegToRad(-90.0f), pitch - stallPitchSpeed * (float)delta * (1.0f - tSpeed));
		}

		Vector3 targetPosition = this.Position + -this.GlobalTransform.Basis.Z * speed * (float)delta;

		if (targetPosition.Y <= startPosition.Y)
		{
			targetPosition.Y = startPosition.Y;
			pitch = Mathf.Max(pitch, 0.0f); // TODO: smooth
		}

		this.Rotation = new Vector3(pitch, yaw, roll);
		this.Position = targetPosition;

		// Play engine sound
		if (engineStreamPlayer != null)
			engineStreamPlayer.PitchScale = 1.0f + (speed / maxSpeed);
	}
}
