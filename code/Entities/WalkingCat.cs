﻿using System;
using Sandbox;

namespace CatHarvest.Entities;

public partial class WalkingCat : AnimatedEntity
{

	private readonly Vector3 minBounds = new Vector3( -800, -770, 0 );
	private readonly Vector3 maxBounds = new Vector3( 750, 790, 0 );
	[Net] public bool IsDying { get; set; } = false;
	public bool Aggressive { get; set; } = false;
	public bool Passive { get; set; } = false;
	public HarvestPlayer Victim { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		PlaybackRate = 0f;

		Tags.Add( "Cat" );

		SetModel( "models/cat/cat.vmdl" );

		HarvestGame.The.AllCats.Add( this );
	}

	RealTimeSince frameTime = 0f;
	float lastDistance = 0f;
	float nextFrame = 0f;
		
	[Event.Tick.Client]
	public void ClientTick()
	{
		float frameDelta = 1f / 24f;
		float minFps = 1f;
		float minDistanceFalloff = 300f;
		float maxDistanceFalloff = 2000f;

		if ( frameTime >= nextFrame )
		{
			CurrentSequence.Time = (CurrentSequence.Time + frameTime) % CurrentSequence.Duration;
			lastDistance = Math.Max( Camera.Position.Distance( Position ) - minDistanceFalloff, 1f );
			nextFrame = frameDelta.LerpTo(minFps, lastDistance / maxDistanceFalloff );

			frameTime = 0f;
		}
	}

	TimeUntil hourOfDeath = 0f;

	public void Snap()
	{
		Particles.Create( "particles/ashes.vpcf", Position );
		hourOfDeath = 0.2f;
		IsDying = true;

		HarvestGame.The.AllCats.Remove( this );
	}

	public bool IsSecret() => HarvestGame.The.SecretCat == this;

	[Event.Tick.Server]
	public void OnTick()
	{
		if ( IsDying )
		{
			RenderColor = RenderColor.WithAlpha( ( hourOfDeath - Time.Now ) * 5 );

			if( hourOfDeath <= 0 )
			{
				Delete();
			}
		}
	}
}