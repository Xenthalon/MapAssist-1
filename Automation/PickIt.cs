﻿using GameOverlay.Drawing;
using MapAssist.Helpers;
using MapAssist.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapAssist.Automation
{
    class PickIt
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private int MAX_RETRY_COUNT;

        private static double _pickRange = 5;

        private BackgroundWorker _worker;
        private Input _input;
        private Inventory _inventory;
        private Movement _movement;

        private IEnumerable<UnitItem> _items;
        private Point _playerPosition;
        private bool _working = false;
        private bool _full = false;

        public bool Busy => _working;
        public bool Full => _full;
        public bool HasWork => _items.Any(x => x.IsDropped &&
                                            (LootFilter.Filter(x).Item1 ||
                                            (!_inventory.IsBeltFull() && (
                                                x.ItemBaseName == "Full Rejuvenation Potion" ||
                                                x.ItemBaseName == "Rejuvenation Potion"
                                            ))));

        public PickIt(BotConfiguration config, Input input, Inventory inventory, Movement movement)
        {
            MAX_RETRY_COUNT = config.Settings.MaxRetries;

            _input = input;
            _inventory = inventory;
            _movement = movement;

            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(PickThings);
            _worker.WorkerSupportsCancellation = true;
        }

        public void Update(GameData gameData)
        {
            if (gameData != null && gameData.PlayerUnit.IsValidPointer && gameData.PlayerUnit.IsValidUnit)
            {
                _items = gameData.AllItems;
                _playerPosition = gameData.PlayerPosition;

                if (_working && !_worker.IsBusy)
                {
                    _worker.RunWorkerAsync();
                }
            }
        }

        public void Run()
        {
            if (!_working)
            {
                _log.Info("Looking for treasure!");
                _working = true;
                _worker.RunWorkerAsync();
            }
            else
            {
                // emergency abort
                _working = false;
            }
        }

        public void Reset()
        {
            _working = false;
            _full = false;
            _worker.CancelAsync();
        }

        private void PickThings(object sender, DoWorkEventArgs e)
        {
            var pickPotions = !_inventory.IsBeltFull();

            var itemsToPick = _items.Where(x => x.IsDropped &&
                                                (LootFilter.Filter(x).Item1 ||
                                                (pickPotions && (
                                                    x.ItemBaseName == "Full Rejuvenation Potion" ||
                                                    x.ItemBaseName == "Rejuvenation Potion"
                                                ))))
                                    .OrderBy(x => Automaton.GetDistance(x.Position, _playerPosition));

            if (itemsToPick.Count() > 0)
            {
                _working = true;

                var item = itemsToPick.First();

                _log.Info($"Picking up {item.ItemBaseName}.");

                var itemId = item.UnitId;
                var picked = false;

                if (Automaton.GetDistance(item.Position, _playerPosition) > _pickRange)
                {
                    _log.Info("Too far away, moving closer...");
                    _movement.TeleportTo(item.Position);

                    do
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    while (Automaton.GetDistance(item.Position, _playerPosition) > _pickRange && _movement.Busy);

                    System.Threading.Thread.Sleep(500);
                }

                for (var i = 0; i < MAX_RETRY_COUNT; i++)
                {
                    _log.Info($"Clicking it {i + 1}/{MAX_RETRY_COUNT}");
                    _input.DoInputAtWorldPosition("{LMB}", item.Position);
                    System.Threading.Thread.Sleep(1000);

                    var refreshedItem = _items.Where(x => x.UnitId == itemId).FirstOrDefault() ?? new UnitItem(IntPtr.Zero);

                    if (refreshedItem.IsValidPointer && (refreshedItem.ItemModeMapped == ItemModeMapped.Inventory || refreshedItem.ItemModeMapped == ItemModeMapped.Belt))
                    {
                        _log.Info("Got it!");
                        picked = true;
                        break;
                    }
                }

                if (!picked)
                {
                    _log.Info("Seems we are full, please help.");
                    _full = true;
                    _working = false;
                }
            }
            else
            {
                _working = false;
            }
        }
    }
}
