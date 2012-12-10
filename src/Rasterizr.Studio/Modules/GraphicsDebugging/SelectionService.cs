﻿using System;
using System.ComponentModel.Composition;
using Rasterizr.Math;
using Rasterizr.Studio.Modules.GraphicsDebugging.ViewModels;

namespace Rasterizr.Studio.Modules.GraphicsDebugging
{
	[Export(typeof(ISelectionService))]
	public class SelectionService : ISelectionService
	{
		public event EventHandler<TracefileFrameChangedEventArgs> SelectedFrameChanged;
		public event EventHandler<TracefileEventChangedEventArgs> SelectedEventChanged;
		public event EventHandler<PixelChangedEventArgs> SelectedPixelChanged;

		private TracefileFrameViewModel _selectedFrame;
		private TracefileEventViewModel _selectedEvent;
		private Point _selectedPixel;

		public TracefileFrameViewModel SelectedFrame
		{
			get { return _selectedFrame; }
			set
			{
				_selectedFrame = value;
				OnSelectedFrameChanged(new TracefileFrameChangedEventArgs(value));
			}
		}

		public TracefileEventViewModel SelectedEvent
		{
			get { return _selectedEvent; }
			set
			{
				_selectedEvent = value;
				OnSelectedEventChanged(new TracefileEventChangedEventArgs(value));
			}
		}

		public Point SelectedPixel
		{
			get { return _selectedPixel; }
			set
			{
				_selectedPixel = value;
				OnSelectedPixelChanged(new PixelChangedEventArgs(value));
			}
		}

		private void OnSelectedFrameChanged(TracefileFrameChangedEventArgs e)
		{
			EventHandler<TracefileFrameChangedEventArgs> handler = SelectedFrameChanged;
			if (handler != null) handler(this, e);
		}

		private void OnSelectedEventChanged(TracefileEventChangedEventArgs e)
		{
			EventHandler<TracefileEventChangedEventArgs> handler = SelectedEventChanged;
			if (handler != null) handler(this, e);
		}

		private void OnSelectedPixelChanged(PixelChangedEventArgs e)
		{
			EventHandler<PixelChangedEventArgs> handler = SelectedPixelChanged;
			if (handler != null) handler(this, e);
		}
	}
}