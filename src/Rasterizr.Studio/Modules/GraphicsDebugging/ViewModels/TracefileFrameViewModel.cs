﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Caliburn.Micro;
using Rasterizr.Diagnostics.Logging;
using Rasterizr.Diagnostics.Logging.ObjectModel;
using Rasterizr.Platform.Wpf;

namespace Rasterizr.Studio.Modules.GraphicsDebugging.ViewModels
{
	public class TracefileFrameViewModel : PropertyChangedBase, IDisposable
	{
		private readonly ISelectionService _selectionService;
		private readonly TracefileFrame _frame;
		private readonly IList<TracefileEventViewModel> _events;

		internal TracefileFrame Model
		{
			get { return _frame; }
		}

		public int Number
		{
			get { return _frame.Number; }
		}

		private BitmapSource _image;
		public BitmapSource Image
		{
			get { return _image; }
			set
			{
				_image = value;
				NotifyOfPropertyChange(() => Image);
			}
		}

		public IList<TracefileEventViewModel> Events
		{
			get { return _events; }
		}

		public TracefileFrameViewModel(ISelectionService selectionService, TracefileFrame frame)
		{
			_selectionService = selectionService;
			_frame = frame;
			_events = _frame.Events.Select(x => new TracefileEventViewModel(x)).ToList();

			_selectionService.SelectedEventChanged += OnSelectedEventChanged;
		}

		private void OnSelectedEventChanged(object sender, TracefileEventChangedEventArgs e)
		{
		    if (_selectionService.SelectedEvent == null)
		        return;

            var swapChainPresenter = new WpfSwapChainPresenter(Dispatcher.CurrentDispatcher);
		    var replayer = new Replayer(
		        _frame, _selectionService.SelectedEvent.Model,
		        swapChainPresenter);

		    Task.Factory.StartNew(() =>
		    {
                replayer.Replay();
                Image = swapChainPresenter.Bitmap;
			});
		}

		public void Dispose()
		{
			_selectionService.SelectedEventChanged -= OnSelectedEventChanged;
		}
	}
}