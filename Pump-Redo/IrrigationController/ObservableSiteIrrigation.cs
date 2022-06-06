using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Pump.IrrigationController
{
    public class ObservableSiteIrrigation
    {
        private readonly ObservableIrrigation _observableIrrigation;

        private readonly Site _site;

        public readonly ObservableCollection<CustomSchedule> CustomScheduleList =
            new ObservableCollection<CustomSchedule>();

        public readonly ObservableCollection<Equipment> EquipmentList = new ObservableCollection<Equipment>();

        public readonly ObservableCollection<ManualSchedule> ManualScheduleList =
            new ObservableCollection<ManualSchedule>();

        public readonly ObservableCollection<Schedule> ScheduleList = new ObservableCollection<Schedule>();
        public readonly ObservableCollection<Sensor> SensorList = new ObservableCollection<Sensor>();
        public readonly ObservableCollection<Site> SiteList = new ObservableCollection<Site>();

        public ObservableSiteIrrigation(ObservableIrrigation observableIrrigation, Site site)
        {
            _site = site;
            _observableIrrigation = observableIrrigation;

            foreach (var equipment in observableIrrigation.EquipmentList.Where(x => _site.Attachments.Contains(x?.ID)))
                EquipmentList.Add(equipment);

            observableIrrigation.EquipmentList.CollectionChanged += EquipmentListOnCollectionChanged;

            foreach (var sensor in observableIrrigation.SensorList.Where(x => _site.Attachments.Contains(x?.ID)))
                SensorList.Add(sensor);

            observableIrrigation.SensorList.CollectionChanged += SensorListOnCollectionChanged;

            foreach (var manualSchedule in observableIrrigation.ManualScheduleList.Where(x =>
                         x.ManualDetails.Any(y => _site.Attachments.Contains(y.id_Equipment))))
                ManualScheduleList.Add(manualSchedule);

            observableIrrigation.ManualScheduleList.CollectionChanged += ManualScheduleListOnCollectionChanged;


            foreach (var schedule in observableIrrigation.ScheduleList.Where(x =>
                         x.ScheduleDetails.Any(y => _site.Attachments.Contains(y.id_Equipment)) ||
                         _site.Attachments.Contains(x.id_Pump)))
                ScheduleList.Add(schedule);

            observableIrrigation.ScheduleList.CollectionChanged += ScheduleListOnCollectionChanged;


            foreach (var customSchedule in observableIrrigation.CustomScheduleList.Where(x =>
                         x.ScheduleDetails.Any(y => _site.Attachments.Contains(y.id_Equipment)) ||
                         _site.Attachments.Contains(x.id_Pump)))
                CustomScheduleList.Add(customSchedule);

            observableIrrigation.CustomScheduleList.CollectionChanged += CustomScheduleListOnCollectionChanged;


            foreach (var addedSite in observableIrrigation.SiteList.Where(x => x?.ID == _site.ID))
                SiteList.Add(addedSite);

            observableIrrigation.SiteList.CollectionChanged += SiteListOnCollectionChanged;
        }

        private void SiteListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    SiteList.Clear();
                    EquipmentList.Clear();
                    SensorList.Clear();
                    ManualScheduleList.Clear();
                    ScheduleList.Clear();
                    CustomScheduleList.Clear();

                    break;
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (Site newItem in e.NewItems)
                    {
                        //TODO Is This Even Possible
                        if (_site.ID != newItem?.ID) continue;
                        SiteList.Add(newItem);
                        foreach (var equipment in _observableIrrigation.EquipmentList.Where(x =>
                                     _site.Attachments.Contains(x?.ID)))
                            EquipmentList.Add(equipment);

                        foreach (var sensor in _observableIrrigation.SensorList.Where(x =>
                                     _site.Attachments.Contains(x?.ID)))
                            SensorList.Add(sensor);

                        foreach (var manualSchedule in _observableIrrigation.ManualScheduleList.Where(x =>
                                     x.ManualDetails.Any(y => _site.Attachments.Contains(y.id_Equipment))))
                            ManualScheduleList.Add(manualSchedule);

                        foreach (var schedule in _observableIrrigation.ScheduleList.Where(x =>
                                     x.ScheduleDetails.Any(y => _site.Attachments.Contains(y.id_Equipment)) ||
                                     _site.Attachments.Contains(x.id_Pump)))
                            ScheduleList.Add(schedule);

                        foreach (var customSchedule in _observableIrrigation.CustomScheduleList.Where(x =>
                                     x.ScheduleDetails.Any(y => _site.Attachments.Contains(y.id_Equipment)) ||
                                     _site.Attachments.Contains(x.id_Pump)))
                            CustomScheduleList.Add(customSchedule);
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (Site newItem in e.NewItems)
                    {
                        if (_site.ID != newItem.ID) continue;
                        _site.Attachments = newItem.Attachments;
                        _site.Description = newItem.Description;
                        _site.NAME = newItem.NAME;

                        //Could Be Add or remove
                        var replaceElement = SiteList.FirstOrDefault(x => x?.ID == newItem.ID);
                        if (replaceElement == null) continue;

                        SiteList[SiteList.IndexOf(replaceElement)] = newItem;

                        //Equipment
                        var newEquipments = _observableIrrigation.EquipmentList.Where(x =>
                            _site.Attachments.Contains(x?.ID) &&
                            EquipmentList.Select(y => y.ID).Contains(x?.ID) == false).ToList();
                        var removeEquipments = EquipmentList.Where(x => _site.Attachments.Contains(x?.ID) == false)
                            .ToList();
                        foreach (var equipment in newEquipments) EquipmentList.Add(equipment);

                        foreach (var removeEquipment in removeEquipments) EquipmentList.Remove(removeEquipment);

                        //Sensor
                        var newSensors = _observableIrrigation.SensorList.Where(x =>
                                _site.Attachments.Contains(x?.ID) &&
                                SensorList.Select(y => y.ID).Contains(x?.ID) == false)
                            .ToList();
                        var removeSensors = SensorList.Where(x => _site.Attachments.Contains(x?.ID) == false).ToList();

                        foreach (var sensor in newSensors) SensorList.Add(sensor);

                        foreach (var removeSensor in removeSensors) SensorList.Remove(removeSensor);


                        //Schedule
                        var newSchedules = _observableIrrigation.ScheduleList.Where(x =>
                            _site.Attachments.Contains(x.id_Pump) ||
                            (x.ScheduleDetails.Any(detail => _site.Attachments.Contains(detail.id_Equipment)) &&
                             ScheduleList.Select(y => y.ID).Contains(x.ID) == false)).ToList();
                        var removeSchedules = ScheduleList.Where(x => _site.Attachments.Contains(x?.ID) == false)
                            .ToList();

                        foreach (var schedule in newSchedules) ScheduleList.Add(schedule);

                        foreach (var removeSchedule in removeSchedules) ScheduleList.Remove(removeSchedule);

                        //CustomSchedule
                        var newCustomSchedules = _observableIrrigation.CustomScheduleList.Where(x =>
                            _site.Attachments.Contains(x.id_Pump) ||
                            (x.ScheduleDetails.Any(detail => _site.Attachments.Contains(detail.id_Equipment)) &&
                             CustomScheduleList.Select(y => y.ID).Contains(x.ID) == false)).ToList();
                        var removeCustomSchedules = CustomScheduleList
                            .Where(x => _site.Attachments.Contains(x?.ID) == false).ToList();

                        foreach (var customSchedule in newCustomSchedules) CustomScheduleList.Add(customSchedule);

                        foreach (var removeCustomSchedule in removeCustomSchedules)
                            CustomScheduleList.Remove(removeCustomSchedule);

                        //ManualSchedule
                        var newManualSchedules = _observableIrrigation.ManualScheduleList.Where(x =>
                            x.ManualDetails.Any(detail => _site.Attachments.Contains(detail.id_Equipment)) &&
                            ManualScheduleList.Select(y => y.ID).Contains(x.ID) == false).ToList();
                        var removeManualSchedules = ManualScheduleList
                            .Where(x => _site.Attachments.Contains(x?.ID) == false).ToList();

                        foreach (var manualSchedule in newManualSchedules) ManualScheduleList.Add(manualSchedule);

                        foreach (var removeManualSchedule in removeManualSchedules)
                            ManualScheduleList.Remove(removeManualSchedule);
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (Site oldItem in e.OldItems)
                    {
                        if (_site.ID != oldItem.ID) continue;

                        SiteList.Remove(oldItem);

                        SiteList.Clear();
                        EquipmentList.Clear();
                        SensorList.Clear();
                        ManualScheduleList.Clear();
                        ScheduleList.Clear();
                        CustomScheduleList.Clear();

                        EquipmentList.Add(null);
                        SensorList.Add(null);
                        ManualScheduleList.Add(null);
                        ScheduleList.Add(null);
                        CustomScheduleList.Add(null);
                    }

                    break;
                }
            }
        }

        private void CustomScheduleListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    CustomScheduleList.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (CustomSchedule newItem in e.NewItems)
                        if (newItem != null && (_site.Attachments.Contains(newItem.id_Pump) ||
                                                newItem.ScheduleDetails.Any(x =>
                                                    _site.Attachments.Contains(x.id_Equipment))))
                            CustomScheduleList.Add(newItem);

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (CustomSchedule newItem in e.NewItems)
                    {
                        if (!_site.Attachments.Contains(newItem.id_Pump) ||
                            !newItem.ScheduleDetails.Any(x => _site.Attachments.Contains(x.id_Equipment))) continue;

                        var replaceElement = CustomScheduleList.FirstOrDefault(x => x?.ID == newItem.ID);
                        if (replaceElement != null)
                            CustomScheduleList[CustomScheduleList.IndexOf(replaceElement)] = newItem;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (CustomSchedule oldItem in e.OldItems) CustomScheduleList.Remove(oldItem);

                    break;
                }
            }
        }

        private void ScheduleListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    ScheduleList.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (Schedule newItem in e.NewItems)
                        if (newItem != null && (_site.Attachments.Contains(newItem.id_Pump) ||
                                                newItem.ScheduleDetails.Any(x =>
                                                    _site.Attachments.Contains(x.id_Equipment))))
                            ScheduleList.Add(newItem);

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (Schedule newItem in e.NewItems)
                    {
                        if (!_site.Attachments.Contains(newItem.id_Pump) ||
                            !newItem.ScheduleDetails.Any(x => _site.Attachments.Contains(x.id_Equipment))) continue;

                        var replaceElement = ScheduleList.FirstOrDefault(x => x?.ID == newItem.ID);
                        if (replaceElement != null)
                            ScheduleList[ScheduleList.IndexOf(replaceElement)] = newItem;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (Schedule oldItem in e.OldItems) ScheduleList.Remove(oldItem);

                    break;
                }
            }
        }


        private void ManualScheduleListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    ManualScheduleList.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (ManualSchedule newItem in e.NewItems)
                        if (newItem != null &&
                            newItem.ManualDetails.Any(x => _site.Attachments.Contains(x.id_Equipment)))
                            ManualScheduleList.Add(newItem);

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (ManualSchedule newItem in e.NewItems)
                    {
                        if (!newItem.ManualDetails.Any(x => _site.Attachments.Contains(x.id_Equipment))) continue;
                        var replaceElement = ManualScheduleList.FirstOrDefault(x => x?.ID == newItem.ID);
                        if (replaceElement != null)
                            ManualScheduleList[ManualScheduleList.IndexOf(replaceElement)] = newItem;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (ManualSchedule oldItem in e.OldItems) ManualScheduleList.Remove(oldItem);

                    break;
                }
            }
        }

        private void SensorListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    SensorList.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (Sensor newItem in e.NewItems)
                        if (newItem != null && _site.Attachments.Contains(newItem.ID))
                            SensorList.Add(newItem);

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (Sensor newItem in e.NewItems)
                    {
                        if (!_site.Attachments.Contains(newItem.ID)) continue;
                        var replaceElement = SensorList.FirstOrDefault(x => x?.ID == newItem.ID);
                        if (replaceElement != null)
                            SensorList[SensorList.IndexOf(replaceElement)] = newItem;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (Sensor oldItem in e.OldItems) SensorList.Remove(oldItem);

                    break;
                }
            }
        }

        private void EquipmentListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    EquipmentList.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (Equipment newItem in e.NewItems)
                        if (newItem != null && _site.Attachments.Contains(newItem.ID))
                            EquipmentList.Add(newItem);

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (Equipment newItem in e.NewItems)
                    {
                        if (!_site.Attachments.Contains(newItem.ID)) continue;
                        var replaceElement = EquipmentList.FirstOrDefault(x => x?.ID == newItem.ID);
                        if (replaceElement != null)
                            EquipmentList[EquipmentList.IndexOf(replaceElement)] = newItem;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (Equipment oldItem in e.OldItems) EquipmentList.Remove(oldItem);

                    break;
                }
            }
        }

        public bool LoadedAllData()
        {
            return !EquipmentList.Contains(null) && !SensorList.Contains(null) && !ManualScheduleList.Contains(null) &&
                   !ScheduleList.Contains(null) && !CustomScheduleList.Contains(null);
        }
    }
}