using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of a <see cref="SelectionBox"/>.
    /// </summary>
    public class SelectionBoxViewModel<T> : BaseViewModel where T : BaseModel, INameable
    {
        #region Delegates and Events

        /// <summary>
        /// Delegate for fireing off event when the <see cref="SelectedElement"/> has changed.
        /// </summary>
        /// <param name="nameable">The <see cref="T"/> model.</param>
        /// <param name="value">True if the element became selected, otherwise false.</param>
        public delegate void SelectionChangedDelegate(T nameable, bool value);

        /// <summary>
        /// Fires off when the selected <see cref="SelectedElement"/> has changed.
        /// </summary>
        public event SelectionChangedDelegate SelectionChanged;

        #endregion

        #region Properties

        #region Collection Model

        private ObservableCollection<T> _collectionModel;

        /// <summary>
        /// The collection model of this <see cref="SelectionBox"/>.
        /// </summary>
        private ObservableCollection<T> CollectionModel
        {
            get { return _collectionModel; }
            set
            {
                if (_collectionModel != null)
                {
                    _collectionModel.CollectionChanged -= OnCollectionChanged;
                }
                _collectionModel = value;
                if (_collectionModel != null)
                {
                    _collectionModel.CollectionChanged += OnCollectionChanged;
                }
            }
        }

        #endregion

        /// <summary>
        /// The elements of the <see cref="SelectionBox"/>.
        /// </summary>
        public ObservableCollection<SelectableElementViewModel<T>> Elements { get; set; } = new ObservableCollection<SelectableElementViewModel<T>>();

        /// <summary>
        /// Indicates whether the <see cref="Elements"/> list is empty or not.
        /// </summary>
        public bool IsElementsListEmpty => Elements.Count == 0;

        #endregion

        #region Methods 

        /// <summary>
        /// Loads the given list of <see cref="INameable"/> as the collection model.
        /// </summary>
        /// <param name="collectionModel">The list of <see cref="INameable"/>.</param>
        public void LoadCollectionModel(ObservableCollection<T> collectionModel)
        {
            CollectionModel = collectionModel;
            RemoveAllElements();
            foreach (var nameable in CollectionModel)
            {
                AddElement(nameable);
            }
        }

        /// <summary>
        /// Adds a <see cref="T"/> model as a new <see cref="SelectableElementViewModel{T}"/> to the <see cref="Elements"/> list.
        /// </summary>
        /// <param name="model">The <see cref="T"/> model.</param>
        private void AddElement(T model)
        {
            var element = new SelectableElementViewModel<T>(model);
            element.SelectionChanged += OnSelectionChanged;
            element.PropertyChanged += OnPropertyChanged;
            Elements.Add(element);
        }

        /// <summary>
        /// Removes an existing <see cref="SelectableElementViewModel{T}"/> from the <see cref="Elements"/> list.
        /// </summary>
        /// <param name="model">The <see cref="T"/> model.</param>
        private void RemoveElement(T model)
        {
            var element = Elements.FirstOrDefault(_ => _.Model == model);
            if (element != null)
            {
                element.SelectionChanged -= OnSelectionChanged;
                element.PropertyChanged -= OnPropertyChanged;
                Elements.Remove(element);
            }
        }

        /// <summary>
        /// Removes all elements from the <see cref="Elements"/> list.
        /// </summary>
        private void RemoveAllElements()
        {
            foreach (var element in Elements)
            {
                element.SelectionChanged -= OnSelectionChanged;
                element.PropertyChanged -= OnPropertyChanged;
            }
            Elements.Clear();
        }

        /// <summary>
        /// Called after the <see cref="CollectionModel"/> has changed.
        /// </summary>
        /// <param name="sender">The <see cref="CollectionModel"/>.</param>
        /// <param name="e">The event args.</param>
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (var element in e.NewItems)
                    {
                        AddElement(element as T);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (var element in e.OldItems)
                    {
                        RemoveElement(element as T);
                    }
                }
            }
            else
            {
                RemoveAllElements();
                if (e.NewItems != null)
                {
                    foreach (var element in e.NewItems)
                    {
                        AddElement(element as T);
                    }
                }
            }
            OnPropertyChanged("");
        }

        /// <summary>
        /// Called whenever a <see cref="SelectableElementViewModel{T}.SelectionChanged"/> event is fired.
        /// </summary>
        /// <param name="element">The <see cref="SelectableElementViewModel{T}"/>.</param>
        private void OnSelectionChanged(SelectableElementViewModel<T> element) => ToggleSelection(element);

        /// <summary>
        /// Toggles the <see cref="SelectableElementViewModel{T}.IsSelected"/> property of a given element.
        /// </summary>
        /// <param name="element">The <see cref="SelectableElementViewModel{T}"/> to select.</param>
        private void ToggleSelection(SelectableElementViewModel<T> element)
        {
            element.IsSelected = !element.IsSelected;
            SelectionChanged?.Invoke(element.Model, element.IsSelected);
        }

        /// <summary>
        /// Selects all elements of this <see cref="SelectionBox"/>.
        /// </summary>
        public void SelectAllElements()
        {
            foreach (var e in Elements)
            {
                if (!e.IsSelected)
                {
                    ToggleSelection(e);
                }
            }
        }


        /// <summary>
        /// Deselects all elements of this <see cref="SelectionBox"/>.
        /// </summary>
        public void DeselectAllElements()
        {
            foreach (var e in Elements)
            {
                if (e.IsSelected)
                {
                    ToggleSelection(e);
                }
            }
        }

        /// <summary>
        /// Gets all the currently <see cref="T"/> models.
        /// </summary>
        public List<T> GetSelectedModels()
        {
            return Elements.Where(_ => _.IsSelected).Select(_ => _.Model).ToList();
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new empty instance of the <see cref="SelectionBoxViewModel{T}"/> class.
        /// </summary>
        public SelectionBoxViewModel() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionBoxViewModel{T}"/> class.
        /// </summary>
        /// <param name="collectionModel">The collection model to load.</param>
        public SelectionBoxViewModel(ObservableCollection<T> collectionModel)
        {
            LoadCollectionModel(collectionModel);
        }

        #endregion
    }
}
