﻿using System.Reactive;
using System.Reactive.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using JapaneseDict.Models;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using JapaneseDict.GUI.Models;
using Windows.Foundation.Collections;

namespace JapaneseDict.GUI.ViewModels
{
    public class NotebookViewModel : ViewModelBase
    {
        public NotebookViewModel()
        {
            IsBusy = false;
            GroupedNoteList = new ObservableCollection<GroupedNoteItem>();
        }
        // If you have install the code sniplets, use "propvm + [tab] +[tab]" create a property
        public async void LoadData()
        {
            IsBusy = true;
            try
            {
                var a = QueryEngine.QueryEngine.NotebookQueryEngine.Get();
                Func<ObservableCollection<GroupedNoteItem>> func = (() => { return new ObservableCollection<GroupedNoteItem>((from item in QueryEngine.QueryEngine.NotebookQueryEngine.Get() orderby item.GroupingKey group item by item.GroupingKey into newItems select new GroupedNoteItem { Key = newItems.Key, ItemContent = newItems.ToList() }).ToList()); });
                GroupedNoteList = await Task.Run(func);
                if (GroupedNoteList.Count == 0)
                {
                    IsNotebookEmpty = true;
                }
                else
                {
                    IsNotebookEmpty = false;
                }
                IsBusy = false;
            }
            catch
            {
                IsBusy = false;
            }
           
        }
        private ObservableCollection<GroupedNoteItem> groupedNoteList;

        public ObservableCollection<GroupedNoteItem> GroupedNoteList
        {
            get { return groupedNoteList; }
            set
            {
                groupedNoteList = value;
                RaisePropertyChanged();
            }
        }
        private RelayCommand<int> _removeFromNotebookCommand;

        /// <summary>
        /// Gets the RemoveFromNoteBookCommand.
        /// </summary>
        public RelayCommand<int> RemoveFromNotebookCommand
        {
            get
            {
                return _removeFromNotebookCommand
                    ?? (_removeFromNotebookCommand = new RelayCommand<int>(
                    (x) =>
                    {
                        QueryEngine.QueryEngine.NotebookQueryEngine.Remove(x);
                        LoadData();
                    }));
            }
        }

        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                RaisePropertyChanged();
            }
        }

        private bool isNotebookEmpty;
        public bool IsNotebookEmpty
        {
            get { return isNotebookEmpty; }
            set
            {
                isNotebookEmpty = value;
                RaisePropertyChanged();
            }
        }      

    }

}

