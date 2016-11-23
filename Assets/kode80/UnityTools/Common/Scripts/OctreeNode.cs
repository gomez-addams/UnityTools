﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace kode80.Common
{
	public class OctreeNode<T>
	{
		public delegate bool ItemOverlapsBounds( T item, Bounds bounds);
		public ItemOverlapsBounds itemOverlapsBounds;

		private OctreeNode<T>[] subNodes;
		public OctreeNode<T>[] SubNodes { get { return subNodes; } }

		private List<T> contents;
		public List<T> Contents { get { return contents; } }

		private Bounds bounds;
		public Bounds Bounds { get { return bounds; } }

		public OctreeNode( Bounds bounds, ItemOverlapsBounds itemOverlapsBounds)
		{
			contents = new List<T>();
			this.bounds = bounds;
			this.itemOverlapsBounds = itemOverlapsBounds;
		}

		public void Add( T item)
		{
			if( itemOverlapsBounds( item, bounds)) 
			{
				if( subNodes == null) {
					contents.Add( item);
				}
				else {
					for( int i=0; i<8; i++) {
						subNodes[i].Add( item);
					}
				}
			}
		}

		public void Add( IList<T> items) 
		{
			int count = items.Count;
			for( int i=0; i<count; i++) {
				Add( items[i]);
			}
		}

		public void Subdivide()
		{
			Bounds bounds = GetSubNodeBounds();
			subNodes = new OctreeNode<T>[8];

			int index=0;
			for( int z=0; z<2; z++) {
				for( int y=0; y<2; y++) {
					for( int x=0; x<2; x++) {
						bounds.center = new Vector3( (bounds.size.x * -0.5f) + (bounds.size.x * x),
													 (bounds.size.y * -0.5f) + (bounds.size.y * y),
													 (bounds.size.z * -0.5f) + (bounds.size.z * z));
						bounds.center += this.bounds.center;
						subNodes[index] = new OctreeNode<T>( bounds, itemOverlapsBounds);
						int count = contents.Count;
						for( int i=0; i<count; i++) {
							subNodes[index].Add( contents[i]);
						}
						index++;
					}
				}
			}

			contents.Clear();
		}

		public List<T> GetAllContents()
		{
			List<T> allContents = new List<T>();
			CollectContents( this, allContents);
			return allContents;
		}

		public void Collapse()
		{
			contents = GetAllContents();
			subNodes = null;
		}

		public List<OctreeNode<T>> GetNodesContainingItem( T item)
		{
			var nodes = new List<OctreeNode<T>>();
			SearchNodesContainingItem( item, nodes);
			return nodes;
		}

		public void DrawGizmo()
		{
			Gizmos.DrawWireCube( bounds.center, bounds.size);
		}

		protected void SearchNodesContainingItem( T item, List<OctreeNode<T>> foundNodes)
		{
			if( itemOverlapsBounds( item, bounds))
			{
				foundNodes.Add( this);

				if( subNodes != null) 
				{
					for( int i=0; i<8; i++) {
						subNodes[i].SearchNodesContainingItem( item, foundNodes);
					}
				}
			}
		}

		protected void CollectContents( OctreeNode<T> node, List<T> allContents)
		{
			allContents.AddRange( node.contents);

			if( node.SubNodes != null)
			{
				for( int i=0; i<8; i++) {
					CollectContents( node.SubNodes[i], allContents);
				}
			}
		}

		private Bounds GetSubNodeBounds()
		{
			Bounds subBounds = new Bounds();
			subBounds.size = bounds.size * 0.5f;
			return subBounds;
		}
	}
}