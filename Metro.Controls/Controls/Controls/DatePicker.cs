using System;
using System.Collections.Generic;
using System.Linq;
using Metro.Controls.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Metro.Controls
{
    [TemplatePart(Name = DaySelectorPartName, Type = typeof (Selector))]
    [TemplatePart(Name = MonthSelectorPartName, Type = typeof (Selector))]
    [TemplatePart(Name = YearSelectorPartName, Type = typeof (Selector))]
    [TemplatePart(Name = DateHelperSelectorPartName, Type = typeof (Selector))]
    public sealed class DatePicker : Control
    {
        public event SelectionChangedEventHandler SelectionChanged;

        public const string DaySelectorPartName = "PART_Day";
        public const string MonthSelectorPartName = "PART_Month";
        public const string YearSelectorPartName = "PART_Year";
        public const string DateHelperSelectorPartName = "PART_DateHelper";

        private bool useShortMonthNames = true;

        private Selector _daySelector;
        private Selector _monthSelector;
        private Selector _yearSelector;

        private Selector _dateHelperSelector;

        private bool isUpdatingSelection;
        private bool isFillingDays;

        public DatePicker()
        {
            this.DefaultStyleKey = typeof (DatePicker);
        }

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue",
                                                                                                      typeof (DateTime),
                                                                                                      typeof(DatePicker),
                                                                                                      PropertyMetadata.Create(GetDefaultValue));
        private static object GetDefaultValue()
        {
            return DateTime.Today;
        }

        private DateTime SelectedDate
        {
            get
            {
                if (SelectedValue == null)
                    return DateTime.Now;

                return (DateTime) SelectedValue;
            }
            set { SelectedValue = value; }
        }

        public DateTime SelectedValue
        {
            get { return (DateTime)GetValue(SelectedValueProperty); }
            set
            {
                SetValueInternal(value);
                UpdateSelection();
            }
        }

        private void SetValueInternal(DateTime newValue)
        {
            DateTime oldValue = SelectedValue;
            SetValue(SelectedValueProperty, newValue);
            RaiseEvent(oldValue, newValue);
        }

        private void RaiseEvent(DateTime oldValue, DateTime newValue)
        {
            var args = new SelectionChangedEventArgs(new List<object> { oldValue }, new List<object> { newValue });
            if (SelectionChanged != null)
                SelectionChanged(this, args);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DetachSelectionChanged();

            _daySelector = GetTemplateChild(DaySelectorPartName) as Selector;
            _monthSelector = GetTemplateChild(MonthSelectorPartName) as Selector;
            _yearSelector = GetTemplateChild(YearSelectorPartName) as Selector;
            _dateHelperSelector = GetTemplateChild(DateHelperSelectorPartName) as Selector;

            if (_daySelector == null)
                throw new Exception("Could not find template part: " + DaySelectorPartName);
            if (_monthSelector == null)
                throw new Exception("Could not find template part: " + MonthSelectorPartName);
            if (_yearSelector == null)
                throw new Exception("Could not find template part: " + YearSelectorPartName);
            if (_dateHelperSelector == null)
                throw new Exception("Could not find template part: " + DateHelperSelectorPartName);

            AttachSelectionChanged();

            FillSelectors();
            UpdateSelection();
        }

        public static DateTime GetEffectiveDate(DatePickerHelpValues value, DateTime relativeToDate)
        {
            switch (value)
            {
                case DatePickerHelpValues.LastWeek:
                    return relativeToDate.FirstDateTimeOfWeek().AddDays(-1).FirstDateTimeOfWeek();

                case DatePickerHelpValues.TwoWeeksAgo:
                    return relativeToDate.FirstDateTimeOfWeek().AddDays(-8).FirstDateTimeOfWeek();

                case DatePickerHelpValues.LastMonth:
                    return relativeToDate.GetFirstDayOfMonth().AddMonths(-1).GetFirstDayOfMonth();

                case DatePickerHelpValues.TwoMonthsAgo:
                    return relativeToDate.GetFirstDayOfMonth().AddMonths(-2).GetFirstDayOfMonth();

                case DatePickerHelpValues.SixMonthsAgo:
                    return relativeToDate.GetFirstDayOfMonth().AddMonths(-6).GetFirstDayOfMonth();

                case DatePickerHelpValues.LastYear:
                    return relativeToDate.GetFirstDayOfYear().AddYears(-1).GetFirstDayOfYear();
            }

            return relativeToDate;
        }

        private void AttachSelectionChanged()
        {
            _daySelector.SelectionChanged += OnDateSelectionChanged;
            _monthSelector.SelectionChanged += OnDateSelectionChanged;
            _yearSelector.SelectionChanged += OnDateSelectionChanged;

            _dateHelperSelector.SelectionChanged += OnDateHelperSelectionChanged;
        }

        private void DetachSelectionChanged()
        {
            if (_daySelector != null)
                _daySelector.SelectionChanged -= OnDateSelectionChanged;

            if (_monthSelector != null)
                _monthSelector.SelectionChanged -= OnDateSelectionChanged;

            if (_monthSelector != null)
                _yearSelector.SelectionChanged -= OnDateSelectionChanged;

            if (_dateHelperSelector != null)
                _dateHelperSelector.SelectionChanged -= OnDateHelperSelectionChanged;
        }

        private void OnDateSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isUpdatingSelection || isFillingDays)
                return;

            //handle changing the list of days displayed

            var newValue = e.AddedItems.FirstOrDefault();
            if (newValue != null)
            {
                if (sender == _monthSelector)
                    AdjustDays(DateTimeExtensions.GetMonthNumber((string)newValue, useShortMonthNames), (int)_yearSelector.SelectedItem);
                else if (sender == _yearSelector)
                    AdjustDays(DateTimeExtensions.GetMonthNumber((string)_monthSelector.SelectedItem, useShortMonthNames), (int)newValue);
            }

            RetrieveSelection();
        }

        private void OnDateHelperSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var helperDescription = (string)_dateHelperSelector.SelectedItem;

            DatePickerHelpValues helperValue = (DatePickerHelpValues)helperDescription.GetEnumValue<DatePickerHelpValues>();

            var dateTimeToSet = GetEffectiveDate(helperValue, DateTime.Now);

            SelectedValue = dateTimeToSet;
        }

        private void AdjustDays(int month, int year)
        {
            var currentSelectedDay = (int)_daySelector.SelectedItem;
            
            //re-fill days list if needed
            DateTime date = new DateTime(year, month, 1);
            EnsureDaysListItems(date);

            //re-select day if needed
            var days = _daySelector.ItemsSource as IEnumerable<int>;
            if (currentSelectedDay > days.Max())
                currentSelectedDay = days.Max();

            _daySelector.SelectedItem = currentSelectedDay;
        }

        private void FillSelectors()
        {
            var baseDate = SelectedDate;

            //Years
            var year = baseDate.Year;
            var years = Enumerable.Range(year - 4, 5); //TODO expose year range externally
            _yearSelector.Items.Clear();
            _yearSelector.ItemsSource = years;

            //Months
            var months = DateTimeExtensions.GetMonthNames(useShortMonthNames);
            _monthSelector.ItemsSource = months;

            //Days
            EnsureDaysListItems(baseDate);

            //Helper
            FillHelperSelector();
        }

        private void FillHelperSelector()
        {
            Type helperEnumType = typeof(DatePickerHelpValues);
            var selectorItems = new List<string>();

            foreach (DatePickerHelpValues value in Enum.GetValues(helperEnumType))
            {
                var name = value.GetDescription();
                var item = name;
                selectorItems.Add(item);
            }

            _dateHelperSelector.ItemsSource = selectorItems;
        }

        private void FillDays(DateTime date)
        {
            isFillingDays = true; //because we don't want another OnDateSelection event to happen when we reset the days list

            var daysInMonth = date.GetDaysInMonth();

            var days = Enumerable.Range(1, daysInMonth);
            _daySelector.ItemsSource = days;

            isFillingDays = false;
        }

        private void EnsureDaysListItems(DateTime date)
        {
            var days = _daySelector.ItemsSource as IEnumerable<int>;
            if (days == null)
            {
                FillDays(date);
            }
            else
            {
                var lastDayInSelector = days.Max();
                if (lastDayInSelector != date.GetDaysInMonth())
                {
                    FillDays(date);
                }
            }
        }

        private void UpdateSelection()
        {
            isUpdatingSelection = true;

            //set UI values

            var selectedDate = SelectedDate;

            EnsureDaysListItems(selectedDate);


            //begin updating UI
            if (_daySelector.SelectedItem == null || (int)_daySelector.SelectedItem != selectedDate.Day)
                _daySelector.SelectedItem = selectedDate.Day;

            var monthName = selectedDate.GetMonthName(useShortMonthNames);
            if (_monthSelector.SelectedItem == null || (string)_monthSelector.SelectedItem != monthName)
                _monthSelector.SelectedItem = monthName;

            if (_yearSelector.SelectedItem == null || (int)_yearSelector.SelectedItem != selectedDate.Year)
                _yearSelector.SelectedItem = selectedDate.Year;

            //end updating UI

            isUpdatingSelection = false;
        }

        private void RetrieveSelection()
        {
            var day = (int)_daySelector.SelectedItem;
            var monthName = (string)_monthSelector.SelectedItem;
            var month = DateTimeExtensions.GetMonthNumber(monthName, useShortMonthNames);
            var year = (int)_yearSelector.SelectedItem;

            //validate:

            var date = new DateTime(year, month, day);

            //SetValueInternal(date);
            SelectedValue = date;
        }
    }

    //TODO: need to add more helpful values (including future dates)
    public enum DatePickerHelpValues
    {
        [LocalisableDescription("last week")]
        LastWeek = 0,
        [LocalisableDescription("2 weeks ago")]
        TwoWeeksAgo = 1,
        [LocalisableDescription("last month")]
        LastMonth = 2,
        [LocalisableDescription("2 months ago")]
        TwoMonthsAgo = 3,
        [LocalisableDescription("6 months ago")]
        SixMonthsAgo = 4,
        [LocalisableDescription("last year")]
        LastYear = 5
    }
}
