﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal {

	public readonly Vector3i Position1;
	public readonly Vector3i Position2;
	public readonly int Direction;

	private readonly OcclusionCulling cullingData;

	public readonly Bounds Bounds;

	public Room Room1 {
		get {
			return this.cullingData.GetRoom(this.Position1);
		}
	}
	public Room Room2 {
		get {
			return this.cullingData.GetRoom(this.Position2);
		}
	}

	public bool IsInside {
		get {
			return this.Room1 != null && this.Room2 != null;
		}
	}

	public Room Room {
		get {
			return this.Room1 ?? this.Room2;
		}
	}

	// Direction must be 0, 1 or 2
	public Portal(Vector3i position, int direction, OcclusionCulling cullingData) {
		this.Position1 = position;
		this.Direction = direction;
		this.Position2 = this.Position1 + Orientations.Direction[direction];
		this.cullingData = cullingData;

		var dir = Orientations.Direction[this.Direction].ToVector3();
		this.Bounds = new Bounds(cullingData.MapBehaviour.GetWorldspacePosition(this.Position1) + dir, (Vector3.one - new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z)) * 0.95f) * 2f);
	}

	public Room Follow(Room currentRoom) {
		if (this.Room1 == currentRoom) {
			return this.Room2;
		} else {
			return this.Room1;
		}
	}

	public bool IsVisibleFromOutside() {
		var normal = Orientations.Direction[this.Direction + (this.Room1 == null ? 3 : 0)].ToVector3();
		return Vector3.Angle(this.cullingData.Camera.transform.forward, -normal) < this.cullingData.Camera.fieldOfView / 2f + 90f
			&& GeometryUtility.TestPlanesAABB(this.cullingData.cameraFrustumPlanes, this.Bounds);
	}

	public bool IsVisibleFromInside() {
		return GeometryUtility.TestPlanesAABB(this.cullingData.cameraFrustumPlanes, this.Bounds);
	}

	public Plane[] GetFrustumPlanes(Vector3 cameraPosition) {
		var planes = new Plane[5];

		Vector3 center = this.Bounds.center;

		var corners = new Vector3[4];

		switch (this.Direction) {
			case 0:
				corners[0] = new Vector3(0, +1, +1);
				corners[1] = new Vector3(0, +1, -1);
				corners[2] = new Vector3(0, -1, -1);
				corners[3] = new Vector3(0, -1, +1);
				break;
			case 1:
				corners[0] = new Vector3(+1, 0, +1);
				corners[1] = new Vector3(+1, 0, -1);
				corners[2] = new Vector3(-1, 0, -1);
				corners[3] = new Vector3(-1, 0, +1);
				break;
			case 2:
				corners[0] = new Vector3(+1, +1, 0);
				corners[1] = new Vector3(+1, -1, 0);
				corners[2] = new Vector3(-1, -1, 0);
				corners[3] = new Vector3(-1, +1, 0);
				break;
		}

		for (int i = 0; i < 4; i++) {
			corners[i] += center;
		}

		for (int i = 0; i < 4; i++) {
			var normal = Vector3.Cross((corners[i] - cameraPosition).normalized, (corners[i] - corners[(i + 1) % 4]).normalized);
			if (Vector3.Dot(normal, (center - corners[i]).normalized) < 0) {
				normal *= -1f;
			}

			planes[i] = new Plane(normal, cameraPosition);
			Debug.DrawLine(cameraPosition, corners[i]);
		}

		var portalNormal = Orientations.Direction[this.Direction].ToVector3();
		if (Vector3.Dot(portalNormal, (center - cameraPosition).normalized) < 0) {
			portalNormal *= -1f;
		}
		planes[4] = new Plane(portalNormal, center);

		return planes;
	}

	public void DrawGizmo(MapBehaviour map, Color color) {
		var pos = 0.5f * (map.GetWorldspacePosition(this.Position1) + map.GetWorldspacePosition(this.Position2));
		var normal = Orientations.Direction[this.Direction + (this.Room1 == null ? 3 : 0)].ToVector3();
		Gizmos.color = color;
		Gizmos.DrawLine(pos, pos + normal.normalized * 0.4f);
	}
}