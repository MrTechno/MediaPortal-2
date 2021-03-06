#region Copyright (C) 2007-2014 Team MediaPortal

/*
    Copyright (C) 2007-2014 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MediaPortal.UI.SkinEngine.Controls.Visuals;
using MediaPortal.UI.SkinEngine.MpfElements;
using MediaPortal.UI.SkinEngine.ScreenManagement;
using MediaPortal.UI.SkinEngine.Utils;
using MediaPortal.Utilities;
using MediaPortal.Utilities.DeepCopy;
using Size = SharpDX.Size2;
using SizeF = SharpDX.Size2F;
using PointF = SharpDX.Vector2;
using RectangleF = SharpDX.RectangleF;

namespace MediaPortal.UI.SkinEngine.Controls.Panels
{
  /// <summary>
  /// This version of the <see cref="WrapPanel"/> enables faster and memory conserving display 
  /// by creating the <see cref="FrameworkElement"/> for items on demand.<br/>
  /// Virtualization is enabled by setting an <see cref="IItemProvider"/> via <see cref="SetItemProvider"/>
  /// and works under the assumption that all items use the same template - therefore are equally sized.
  /// </summary>
  public class VirtualizingWrapPanel : WrapPanel, IVirtualizingPanel
	{
    #region Protected fields

    protected IItemProvider _itemProvider = null;
    protected IItemProvider _newItemProvider = null; // Store new item provider until next render cylce

    // Assigned in ArrangeChildren
    protected FrameworkElement[] _arrangedItems = null;
    protected int _firstArrangedLineIndex = -1;
    protected int _lastArrangedLineIndex = -1;

    // Assigned in ArrangeChildren and CalculateInnerDesiredSize
    protected float _assumedLineExtendsInNonOrientationDirection = 0;

    #endregion

    #region Ctor

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      VirtualizingWrapPanel p = (VirtualizingWrapPanel)source;
      _itemProvider = copyManager.GetCopy(p._itemProvider);
      _assumedLineExtendsInNonOrientationDirection = 0;
      _arrangedItems = null;
      _firstArrangedLineIndex = -1;
      _lastArrangedLineIndex = -1;
    }

    public override void Dispose()
    {
      base.Dispose();
      IItemProvider itemProvider = _itemProvider;
      _itemProvider = null;
      if (itemProvider != null)
        MPF.TryCleanupAndDispose(itemProvider);
      itemProvider = _newItemProvider;
      _newItemProvider = null;
      if (itemProvider != null)
        MPF.TryCleanupAndDispose(itemProvider);
    }

    #endregion

    #region IVirtualizingPanel implementation

    public IItemProvider ItemProvider
    {
      get { return _itemProvider; }
    }

    public bool IsVirtualizing
    {
      get { return _itemProvider != null; }
    }

    public void SetItemProvider(IItemProvider itemProvider)
    {
      if (_elementState == ElementState.Running)
        lock (Children.SyncRoot)
        {
          if (_newItemProvider == itemProvider)
            return;
          if (_newItemProvider != null)
            MPF.TryCleanupAndDispose(_newItemProvider);
          _newItemProvider = null;
          if (_itemProvider == itemProvider)
            return;
          _newItemProvider = itemProvider;
        }
      else
      {
        if (_newItemProvider == itemProvider)
          return;
        if (_newItemProvider != null)
          MPF.TryCleanupAndDispose(_newItemProvider);
        _newItemProvider = null;
        if (_itemProvider == itemProvider)
          return;
        if (_itemProvider != null)
          MPF.TryCleanupAndDispose(_itemProvider);
        _itemProvider = itemProvider;
      }
      InvalidateLayout(true, true);
    }

    #endregion

    #region Layouting

    public override void SetScrollIndex(int lineIndex, bool first)
    {
      lock (Children.SyncRoot)
      {
        if (_pendingScrollIndex == lineIndex && _scrollToFirst == first ||
            (!_pendingScrollIndex.HasValue &&
             ((_scrollToFirst && _actualFirstVisibleLineIndex == lineIndex) ||
              (!_scrollToFirst && _actualLastVisibleLineIndex == lineIndex))))
          return;
        _pendingScrollIndex = lineIndex;
        _scrollToFirst = first;
      }
      InvalidateLayout(true, true);
    }

    /// <summary>
    /// A copy of the <see cref="WrapPanel.CalculateLine"/> method, but using the <see cref="ItemProvider"/>
    /// for item retrieval.
    /// </summary>
    protected LineMeasurement CalculateLine(int startIndex, SizeF? measureSize, bool invertLayoutDirection)
    {
      LineMeasurement result = LineMeasurement.Create();
      if (invertLayoutDirection)
        result.EndIndex = startIndex;
      else
        result.StartIndex = startIndex;
      result.TotalExtendsInNonOrientationDirection = 0;
      int numChildren = ItemProvider.NumItems;
      int directionOffset = invertLayoutDirection ? -1 : 1;
      float offsetInOrientationDirection = 0;
      float extendsInOrientationDirection = measureSize.HasValue ? GetExtendsInOrientationDirection(Orientation, measureSize.Value) : float.NaN;
      int currentIndex = startIndex;
      for (; invertLayoutDirection ? (currentIndex >= 0) : (currentIndex < numChildren); currentIndex += directionOffset)
      {
        FrameworkElement child = GetElement(currentIndex);
        SizeF desiredChildSize;
        if (measureSize.HasValue)
        {
          desiredChildSize = measureSize.Value;
          child.Measure(ref desiredChildSize);
        }
        else
          desiredChildSize = child.DesiredSize;
        float lastOffsetInOrientationDirection = offsetInOrientationDirection;
        offsetInOrientationDirection += GetExtendsInOrientationDirection(Orientation, desiredChildSize);
        if (!float.IsNaN(extendsInOrientationDirection) && offsetInOrientationDirection > extendsInOrientationDirection)
        {
          if (invertLayoutDirection)
            result.StartIndex = currentIndex + 1;
          else
            result.EndIndex = currentIndex - 1;
          result.TotalExtendsInOrientationDirection = lastOffsetInOrientationDirection;
          return result;
        }
        if (desiredChildSize.Height > result.TotalExtendsInNonOrientationDirection)
          result.TotalExtendsInNonOrientationDirection = GetExtendsInNonOrientationDirection(Orientation, desiredChildSize);
      }
      if (invertLayoutDirection)
        result.StartIndex = currentIndex + 1;
      else
        result.EndIndex = currentIndex - 1;
      result.TotalExtendsInOrientationDirection = offsetInOrientationDirection;
      return result;
    }

    /// <summary>
    /// A copy of the <see cref="WrapPanel.LayoutLine"/> method, but using the <see cref="ItemProvider"/>
    /// via the <see cref="GetItem"/> method for item retrieval.
    /// </summary>
    protected void LayoutLine(PointF pos, LineMeasurement line)
    {
      float offset = 0;
      for (int i = line.StartIndex; i <= line.EndIndex; i++)
      {
        FrameworkElement layoutChild = GetItem(i, ItemProvider, true);
        SizeF desiredChildSize = layoutChild.DesiredSize;
        SizeF size;
        PointF location;

        if (Orientation == Orientation.Horizontal)
        {
          size = new SizeF(desiredChildSize.Width, line.TotalExtendsInNonOrientationDirection);
          location = new PointF(pos.X + offset, pos.Y);
          ArrangeChildVertical(layoutChild, layoutChild.VerticalAlignment, ref location, ref size);
          offset += desiredChildSize.Width;
        }
        else
        {
          size = new SizeF(line.TotalExtendsInNonOrientationDirection, desiredChildSize.Height);
          location = new PointF(pos.X, pos.Y + offset);
          ArrangeChildHorizontal(layoutChild, layoutChild.HorizontalAlignment, ref location, ref size);
          offset += desiredChildSize.Height;
        }

        layoutChild.Arrange(new RectangleF(location.X, location.Y, size.Width, size.Height));

        _arrangedItems[i] = layoutChild;
      }
    }

    protected override SizeF CalculateInnerDesiredSize(SizeF totalSize)
    {
      FrameworkElementCollection children = Children;
      lock (Children.SyncRoot)
      {
        if (_newItemProvider != null)
        {
          if (children.Count > 0)
            children.Clear(false);
          if (_itemProvider != null)
            MPF.TryCleanupAndDispose(_itemProvider);
          _itemProvider = _newItemProvider;
          _newItemProvider = null;
          _updateRenderOrder = true;
        }
        _assumedLineExtendsInNonOrientationDirection = 0;
        IItemProvider itemProvider = ItemProvider;
        if (itemProvider == null)
          return base.CalculateInnerDesiredSize(totalSize);
        int numItems = itemProvider.NumItems;
        if (numItems == 0)
          return new SizeF();

        // CalculateInnerDesiredSize is called before ArrangeChildren!
        // under the precondition that all items use the same template and are equally sized 
        // calulate just one line to find number of items and required size of a line
        LineMeasurement exemplaryLine = _firstArrangedLineIndex < 0 ? CalculateLine(0, totalSize, false) : _arrangedLines[_firstArrangedLineIndex];
        _assumedLineExtendsInNonOrientationDirection = exemplaryLine.TotalExtendsInNonOrientationDirection;
        var itemsPerLine = exemplaryLine.EndIndex - exemplaryLine.StartIndex + 1;
        var estimatedExtendsInNonOrientationDirection = (float)Math.Ceiling((float)numItems / itemsPerLine) * _assumedLineExtendsInNonOrientationDirection;

        return Orientation == Orientation.Horizontal ? new SizeF(exemplaryLine.TotalExtendsInOrientationDirection, estimatedExtendsInNonOrientationDirection) :
            new SizeF(estimatedExtendsInNonOrientationDirection, exemplaryLine.TotalExtendsInOrientationDirection);
      }
    }

    protected FrameworkElement GetItem(int childIndex, IItemProvider itemProvider, bool forceMeasure)
    {
      lock (Children.SyncRoot)
      {
        bool newlyCreated;
        FrameworkElement item = itemProvider.GetOrCreateItem(childIndex, this, out newlyCreated);
        if (item == null)
          return null;
        if (newlyCreated)
        {
          // VisualParent and item.Screen were set by the item provider
          item.SetElementState(ElementState.Preparing);
          if (_elementState == ElementState.Running)
            item.SetElementState(ElementState.Running);
        }
        if (newlyCreated || forceMeasure)
        {
          SizeF childSize = Orientation == Orientation.Vertical ? new SizeF((float)ActualWidth, float.NaN) :
              new SizeF(float.NaN, (float)ActualHeight);
          item.Measure(ref childSize);
        }
        return item;
      }
    }

    protected override void ArrangeChildren()
    {
      bool fireScrolled = false;
      lock (Children.SyncRoot)
      {
        if (ItemProvider == null)
        {
          base.ArrangeChildren();
          return;
        }

        _totalHeight = 0;
        _totalWidth = 0;
        int numItems = ItemProvider.NumItems;
        if (numItems > 0)
        {
          PointF actualPosition = ActualPosition;
          SizeF actualSize = new SizeF((float)ActualWidth, (float)ActualHeight);

          // For Orientation == vertical, this is ActualHeight, for horizontal it is ActualWidth
          float actualExtendsInOrientationDirection = GetExtendsInOrientationDirection(Orientation, actualSize);
          // For Orientation == vertical, this is ActualWidth, for horizontal it is ActualHeight
          float actualExtendsInNonOrientationDirection = GetExtendsInNonOrientationDirection(Orientation, actualSize);
          // Hint: We cannot skip the arrangement of lines above _actualFirstVisibleLineIndex or below _actualLastVisibleLineIndex
          // because the rendering and focus system also needs the bounds of the currently invisible children
          float startPosition = 0;
          // If set to true, we'll check available space from the last to first visible child.
          // That is necessary if we want to scroll a specific child to the last visible position.
          bool invertLayouting = false;
          lock (_renderLock)
            if (_pendingScrollIndex.HasValue)
            {
              fireScrolled = true;
              int pendingSI = _pendingScrollIndex.Value;
              if (_scrollToFirst)
                _actualFirstVisibleLineIndex = pendingSI;
              else
              {
                _actualLastVisibleLineIndex = pendingSI;
                invertLayouting = true;
              }
              _pendingScrollIndex = null;
            }

          int itemsPerLine = 0;
          // if we haven't arranged any lines previously - items per line can't be calculated yet, but is not needed
          if (_firstArrangedLineIndex >= 0)
          {
            // to calculate the starting index of elements for a line we assume that every line (until the last) has the same number of items
            itemsPerLine = _arrangedLines[_firstArrangedLineIndex].EndIndex - _arrangedLines[_firstArrangedLineIndex].StartIndex + 1;
            _assumedLineExtendsInNonOrientationDirection = _arrangedLines[_firstArrangedLineIndex].TotalExtendsInNonOrientationDirection;
          }

          // scrolling may have set an invalid index for the first visible line
          if (_actualFirstVisibleLineIndex < 0) _actualFirstVisibleLineIndex = 0;
          if (itemsPerLine > 0)
          {
            int linesPerPage = (int)Math.Floor(actualExtendsInNonOrientationDirection / _assumedLineExtendsInNonOrientationDirection);
            int maxLineIndex = (int)Math.Ceiling((float)numItems / itemsPerLine);
            if (_actualFirstVisibleLineIndex > maxLineIndex - linesPerPage)
            {
              _actualFirstVisibleLineIndex = maxLineIndex - linesPerPage;
            }
          }

          // clear values from previous arrange
          _arrangedItems = new FrameworkElement[numItems];
          _arrangedLines.Clear();
          _firstArrangedLineIndex = 0;
          _lastArrangedLineIndex = 0;

          // 1) Calculate scroll indices
          if (_doScroll)
          { // Calculate last visible child
            float spaceLeft = actualExtendsInNonOrientationDirection;
            if (invertLayouting)
            {
              if (_actualLastVisibleLineIndex == int.MaxValue) // when scroll to last item (END) was requested
                _actualLastVisibleLineIndex = (int)Math.Ceiling((float)numItems / itemsPerLine) - 1;
              _actualFirstVisibleLineIndex = _actualLastVisibleLineIndex + 1;
              int currentLineIndex = _actualLastVisibleLineIndex;
              _lastArrangedLineIndex = currentLineIndex;
              while (_arrangedLines.Count <= currentLineIndex) _arrangedLines.Add(new LineMeasurement()); // add "unarranged lines" up to the last visible
              int itemIndex = currentLineIndex * itemsPerLine;
              int additionalLinesBefore = 0;
              while (currentLineIndex >= 0 && additionalLinesBefore < NUM_ADD_MORE_FOCUS_LINES)
              {
                LineMeasurement line = CalculateLine(itemIndex, _innerRect.Size, false);
                _arrangedLines[currentLineIndex] = line;

                _firstArrangedLineIndex = currentLineIndex;

                currentLineIndex--;
                itemIndex = line.StartIndex - itemsPerLine;
                
                spaceLeft -= line.TotalExtendsInNonOrientationDirection;
                if (spaceLeft + DELTA_DOUBLE < 0)
                  additionalLinesBefore++;
                else
                  _actualFirstVisibleLineIndex--;
              }
              // now add NUM_ADD_MORE_FOCUS_LINES after last visible
              itemIndex = _arrangedLines[_lastArrangedLineIndex].EndIndex + 1;
              int additionalLinesAfterwards = 0;
              while (itemIndex < numItems && additionalLinesAfterwards < NUM_ADD_MORE_FOCUS_LINES)
              {
                LineMeasurement line = CalculateLine(itemIndex, _innerRect.Size, false);
                _arrangedLines.Add(line);
                _lastArrangedLineIndex = _arrangedLines.Count - 1;
                itemIndex = line.EndIndex + 1;
                additionalLinesAfterwards++;
              }
            }
            else
            {
              _actualLastVisibleLineIndex = _actualFirstVisibleLineIndex - 1;
              _firstArrangedLineIndex = Math.Max(_actualFirstVisibleLineIndex - NUM_ADD_MORE_FOCUS_LINES, 0);
              int currentLineIndex = _firstArrangedLineIndex;
              // add "unarranges lines" up until where we start
              while (_arrangedLines.Count < currentLineIndex) _arrangedLines.Add(new LineMeasurement()); 
              int itemIndex = currentLineIndex * itemsPerLine;
              int additionalLinesAfterwards = 0;
              while (itemIndex < numItems && additionalLinesAfterwards < NUM_ADD_MORE_FOCUS_LINES)
              {
                LineMeasurement line = CalculateLine(itemIndex, _innerRect.Size, false);                
                _arrangedLines.Add(line);
                
                _lastArrangedLineIndex = currentLineIndex;

                currentLineIndex++;
                itemIndex = line.EndIndex + 1;

                if (currentLineIndex > _actualFirstVisibleLineIndex)
                {
                  spaceLeft -= line.TotalExtendsInNonOrientationDirection;
                  if (spaceLeft + DELTA_DOUBLE < 0)
                    additionalLinesAfterwards++;
                  else
                    _actualLastVisibleLineIndex++;
                }
              }
            }
          }
          else
          {
            _actualFirstVisibleLineIndex = 0;
            _actualLastVisibleLineIndex = _arrangedLines.Count - 1;
          }

          // now we know items per line for sure so just calculate it
          itemsPerLine = _arrangedLines[_firstArrangedLineIndex].EndIndex - _arrangedLines[_firstArrangedLineIndex].StartIndex + 1;
          _assumedLineExtendsInNonOrientationDirection = _arrangedLines[_firstArrangedLineIndex].TotalExtendsInNonOrientationDirection;
          
          // 2) Calculate start position (so the first visible line starts at 0)
          startPosition -= (_actualFirstVisibleLineIndex - _firstArrangedLineIndex) * _assumedLineExtendsInNonOrientationDirection;

          // 3) Arrange children
          if (Orientation == Orientation.Vertical)
            _totalHeight = actualExtendsInOrientationDirection;
          else
            _totalWidth = actualExtendsInOrientationDirection;
          PointF position = Orientation == Orientation.Vertical ?
              new PointF(actualPosition.X + startPosition, actualPosition.Y) :
              new PointF(actualPosition.X, actualPosition.Y + startPosition);
          foreach (LineMeasurement line in _arrangedLines.Skip(_firstArrangedLineIndex).Take(_lastArrangedLineIndex - _firstArrangedLineIndex + 1))
          {
            LayoutLine(position, line);

            startPosition += line.TotalExtendsInNonOrientationDirection;
            if (Orientation == Orientation.Vertical)
              position = new PointF(actualPosition.X + startPosition, actualPosition.Y);
            else
              position = new PointF(actualPosition.X, actualPosition.Y + startPosition);
          }

          // estimate the desired size
          var estimatedExtendsInNonOrientationDirection = (float)Math.Ceiling((float)numItems / itemsPerLine) * _assumedLineExtendsInNonOrientationDirection;
          if (Orientation == Orientation.Horizontal)
            _totalHeight = estimatedExtendsInNonOrientationDirection;
          else
            _totalWidth = estimatedExtendsInNonOrientationDirection;

          // keep one more item, because we did use it in CalcLine (and need always one more to find the last item not fitting on the line)
          // -> if we dont, it will always be newlyCreated and we keep calling Arrange since the new item recursively sets the parent invalid
          _itemProvider.Keep(_arrangedLines[_firstArrangedLineIndex].StartIndex, _arrangedLines[_lastArrangedLineIndex].EndIndex + 1);
        }
        else
        {
          _actualFirstVisibleLineIndex = 0;
          _actualLastVisibleLineIndex = -1;
        }
      }
      if (fireScrolled)
        InvokeScrolled();
    }

    public override void BringIntoView(int index)
    {
      IItemProvider itemProvider = ItemProvider;
      if (itemProvider == null)
      {
        base.BringIntoView(index);
        return;
      }
      if (_doScroll)
      {
        int itemsPerLine = _arrangedLines[_firstArrangedLineIndex].EndIndex - _arrangedLines[_firstArrangedLineIndex].StartIndex + 1;
        int line = index / itemsPerLine;

        if (index < _arrangedLines[_actualFirstVisibleLineIndex].StartIndex)
        {
          SetScrollIndex(line, true);
        }
        else if (index > _arrangedLines[_actualLastVisibleLineIndex].EndIndex)
        {
          SetScrollIndex(line, false);
        }
      }
    }

    protected override void MakeChildVisible(UIElement element, ref RectangleF elementBounds)
    {
      IItemProvider itemProvider = ItemProvider;
      if (itemProvider == null)
      {
        base.MakeChildVisible(element, ref elementBounds);
        return;
      }

      if (_doScroll)
      {
        int lineIndex = 0;

        IList<FrameworkElement> arrangedItemsCopy;
        lock (Children.SyncRoot)
        {
          arrangedItemsCopy = new List<FrameworkElement>(_arrangedItems);
        }
        IList<LineMeasurement> lines = new List<LineMeasurement>(_arrangedLines);
        foreach (LineMeasurement line in lines)
        {
          for (int childIndex = line.StartIndex; childIndex <= line.EndIndex; childIndex++)
          {
            FrameworkElement currentChild = arrangedItemsCopy[childIndex];
            if (InVisualPath(currentChild, element))
            {
              int oldFirstVisibleLine = _actualFirstVisibleLineIndex;
              int oldLastVisibleLine = _actualLastVisibleLineIndex;
              bool first;
              if (lineIndex < oldFirstVisibleLine)
                first = true;
              else if (lineIndex <= oldLastVisibleLine)
                // Already visible
                break;
              else
                first = false;
              SetScrollIndex(lineIndex, first);
              // Adjust the scrolled element's bounds; Calculate the difference between positions of childen at old/new child indices
              float extendsInOrientationDirection = SumActualLineExtendsInNonOrientationDirection(lines,
                  first ? oldFirstVisibleLine : oldLastVisibleLine, lineIndex);
              if (Orientation == Orientation.Horizontal)
                elementBounds.X -= extendsInOrientationDirection;
              else
                elementBounds.Y -= extendsInOrientationDirection;
              break;
            }
          }
          lineIndex++;
        }
      }
    }

    public override FrameworkElement GetElement(int index)
    {
      IItemProvider itemProvider = ItemProvider;
      if (itemProvider == null)
        return base.GetElement(index);

      lock (Children.SyncRoot)
        return GetItem(index, itemProvider, true);
    }

    public override void AddChildren(ICollection<UIElement> childrenOut)
    {
      IItemProvider itemProvider = ItemProvider;
      if (itemProvider == null)
      {
        base.AddChildren(childrenOut);
        return;
      }
      if (_arrangedItems != null)
        lock (Children.SyncRoot)
          CollectionUtils.AddAll(childrenOut, _arrangedItems.Where(i => i != null));
    }

    #endregion

    #region Rendering

    protected override IEnumerable<FrameworkElement> GetRenderedChildren()
    {
      if (ItemProvider == null)
        return base.GetRenderedChildren();

      if (_actualFirstVisibleLineIndex < 0 || _actualLastVisibleLineIndex < _actualFirstVisibleLineIndex)
        return new List<FrameworkElement>();

      int start = _arrangedLines[_actualFirstVisibleLineIndex].StartIndex;
      int end = _arrangedLines[_actualLastVisibleLineIndex].EndIndex;
      return _arrangedItems.Skip(start).Take(end - start + 1);
    }

    #endregion

    #region Focus handling overrides

    public override void AlignedPanelAddPotentialFocusNeighbors(RectangleF? startingRect, ICollection<FrameworkElement> outElements,
        bool linesBeforeAndAfter)
    {
      if (ItemProvider == null)
      {
        base.AlignedPanelAddPotentialFocusNeighbors(startingRect, outElements, linesBeforeAndAfter);
        return;
      }
      if (!IsVisible)
        return;
      if (Focusable)
        outElements.Add(this);

      IList<FrameworkElement> arrangedItemsCopy;
      lock (Children.SyncRoot)
      {
        arrangedItemsCopy = new List<FrameworkElement>(_arrangedItems.Skip(_arrangedLines[0].StartIndex).Take(_arrangedLines[_arrangedLines.Count - 1].EndIndex + 1));
      }
      int numLinesBeforeAndAfter = linesBeforeAndAfter ? NUM_ADD_MORE_FOCUS_LINES : 0;
      AddFocusedElementRange(arrangedItemsCopy, startingRect, _actualFirstVisibleLineIndex, _actualLastVisibleLineIndex,
          numLinesBeforeAndAfter, numLinesBeforeAndAfter, outElements);
    }

    public override bool FocusPageUp()
    {
      IItemProvider itemProvider = ItemProvider;
      if (itemProvider == null)
        return base.FocusPageUp();

      if (Orientation == Orientation.Horizontal)
      {
        FrameworkElement currentElement = GetFocusedElementOrChild();
        if (currentElement == null)
          return false;
        if (itemProvider.NumItems == 0)
          return false;
        IList<LineMeasurement> lines = new List<LineMeasurement>(_arrangedLines);
        if (lines.Count == 0)
          return false;
        int firstVisibleLineIndex = _actualFirstVisibleLineIndex;
        LineMeasurement firstVisibleLine = lines[firstVisibleLineIndex];
        float limitPosition = ActualPosition.Y; // Initialize as if an element inside our visible range is focused - then, we must move to the first element
        for (int childIndex = firstVisibleLine.StartIndex; childIndex <= firstVisibleLine.EndIndex; childIndex++)
        {
          FrameworkElement child = _arrangedItems[childIndex];
          if (!InVisualPath(child, currentElement))
            continue;
          // One of the topmost elements is focused - move one page up
          // a) how many lines to go up?
          int numLinesToGoUp = (int)Math.Floor(ActualHeight / firstVisibleLine.TotalExtendsInNonOrientationDirection) - 1;
          if (numLinesToGoUp < 1) numLinesToGoUp = 1;
          // b) what child to select?
          int childIndexToSelect = childIndex - ((firstVisibleLine.EndIndex - firstVisibleLine.StartIndex + 1) * numLinesToGoUp);
          if (childIndexToSelect < 0) childIndexToSelect = 0;
          // c) line index to scroll to
          int lineToScrollTo = firstVisibleLineIndex - numLinesToGoUp;
          if (lineToScrollTo < 0) lineToScrollTo = 0;
          SetScrollIndex(lineToScrollTo, true);
          FrameworkElement item = GetItem(childIndexToSelect, itemProvider, false);
          if (item != null)
            item.SetFocusPrio = SetFocusPriority.Default;
          return true;
        }
        FrameworkElement nextElement;
        while ((nextElement = FindNextFocusElement(lines.Skip(firstVisibleLineIndex).Take(1).SelectMany(
            line => _arrangedItems.Skip(line.StartIndex).Take(line.EndIndex - line.StartIndex + 1)),
            currentElement.ActualBounds, MoveFocusDirection.Up)) != null && (nextElement.ActualPosition.Y > limitPosition - DELTA_DOUBLE))
          currentElement = nextElement;
        return currentElement.TrySetFocus(true);
      }
      return false;
    }

    public override bool FocusPageDown()
    {
      IItemProvider itemProvider = ItemProvider;
      if (itemProvider == null)
        return base.FocusPageDown();

      if (Orientation == Orientation.Horizontal)
      {
        FrameworkElement currentElement = GetFocusedElementOrChild();
        if (currentElement == null)
          return false;
        if (itemProvider.NumItems == 0)
          return false;
        IList<LineMeasurement> lines = new List<LineMeasurement>(_arrangedLines);
        if (lines.Count == 0)
          return false;
        int lastVisibleLineIndex = _actualLastVisibleLineIndex;
        CalcHelper.Bound(ref lastVisibleLineIndex, 0, lines.Count - 1);
        LineMeasurement lastVisibleLine = lines[lastVisibleLineIndex];
        float limitPosition = ActualPosition.Y + (float)ActualHeight; // Initialize as if an element inside our visible range is focused - then, we must move to the last element
        for (int childIndex = lastVisibleLine.StartIndex; childIndex <= lastVisibleLine.EndIndex; childIndex++)
        {
          FrameworkElement child = _arrangedItems[childIndex];
          if (!InVisualPath(child, currentElement))
            continue;

          // One of the elements at the bottom is focused - move one page down 
          // so the current last visible line is then the first visible line
          // the new line and item to select might not yet have been arranged
          // calculate the position by assuming all lines have equal amount of items

          // go as many lines down as fill one page (minus 1)
          int numLinesToGoDown = (int)Math.Floor(ActualHeight / lastVisibleLine.TotalExtendsInNonOrientationDirection) - 1;
          int highestPossibleLineIndex = itemProvider.NumItems / (lastVisibleLine.EndIndex - lastVisibleLine.StartIndex + 1);
          if (lastVisibleLineIndex + numLinesToGoDown > highestPossibleLineIndex) numLinesToGoDown = highestPossibleLineIndex - lastVisibleLineIndex;
          int lineToScrollTo = lastVisibleLineIndex + numLinesToGoDown;
          SetScrollIndex(lineToScrollTo, false);
          // try to select a child at same horizontal position
          int childIndexToSelect = childIndex + ((lastVisibleLine.EndIndex - lastVisibleLine.StartIndex + 1) * numLinesToGoDown);
          if (childIndexToSelect > itemProvider.NumItems - 1) childIndexToSelect = itemProvider.NumItems - 1;
          FrameworkElement item = GetItem(childIndexToSelect, itemProvider, false);
          if (item != null)
            item.SetFocusPrio = SetFocusPriority.Default;
          return true;
        }
        // select item on same page in last visible line at same horizontal position
        int lastMinusOne = lastVisibleLineIndex - 1;
        CalcHelper.Bound(ref lastMinusOne, 0, lines.Count - 1);
        FrameworkElement nextElement;
        while ((nextElement = FindNextFocusElement(lines.Skip(lastMinusOne).SelectMany(
            line => _arrangedItems.Skip(line.StartIndex).Take(line.EndIndex - line.StartIndex + 1)),
            currentElement.ActualBounds, MoveFocusDirection.Down)) != null && (nextElement.ActualBounds.Bottom < limitPosition + DELTA_DOUBLE))
          currentElement = nextElement;
        return currentElement.TrySetFocus(true);
      }
      return false;
    }

    public override bool FocusHome()
    {
      IItemProvider itemProvider = ItemProvider;
      if (itemProvider == null)
        return base.FocusHome();

      lock (Children.SyncRoot)
      {
        if (itemProvider.NumItems == 0)
          return false;
        FrameworkElement item = GetItem(0, itemProvider, false);
        if (item != null)
          item.SetFocusPrio = SetFocusPriority.Default;
      }
      SetScrollIndex(0, true);
      return true;
    }

    public override bool FocusEnd()
    {
      IItemProvider itemProvider = ItemProvider;
      if (itemProvider == null)
        return base.FocusHome();

      int numItems;
      lock (Children.SyncRoot)
      {
        numItems = itemProvider.NumItems;
        if (numItems == 0)
          return false;
        FrameworkElement item = GetItem(numItems - 1, itemProvider, false);
        if (item != null)
          item.SetFocusPrio = SetFocusPriority.Default;
      }
      SetScrollIndex(int.MaxValue, false);
      return true;
    }

    #endregion

    #region IScrollInfo implementation overrides

    public override float ViewPortStartX
    {
      get { return Orientation == Orientation.Horizontal ? 0 : _actualFirstVisibleLineIndex * _assumedLineExtendsInNonOrientationDirection; }
    }

    public override float ViewPortStartY
    {
      get { return Orientation == Orientation.Vertical ? 0 : _actualFirstVisibleLineIndex * _assumedLineExtendsInNonOrientationDirection; }
    }

    public override bool IsViewPortAtBottom
    {
      get { return Orientation == Orientation.Vertical || _actualLastVisibleLineIndex >= _arrangedLines.Count - 1; }
    }

    public override bool IsViewPortAtRight
    {
      get { return Orientation == Orientation.Horizontal || _actualLastVisibleLineIndex >= _arrangedLines.Count - 1; }
    }

    #endregion
  }
}
