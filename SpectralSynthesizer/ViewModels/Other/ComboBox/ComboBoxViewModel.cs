using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of a <see cref="ComboBox"/>.
    /// </summary>
    public class ComboBoxViewModel<T> : BaseViewModel where T : BaseModel, INameable
    {
        #region Delegates and Events

        /// <summary>
        /// Delegate for fireing off event when the <see cref="SelectedElement"/> has changed.
        /// </summary>
        /// <param name="nameable">The <see cref="T"/> model.</param>
        public delegate void SelectionChangedDelegate(T nameable);

        /// <summary>
        /// Fires off when the selected <see cref="SelectedElement"/> has changed.
        /// </summary>
        public event SelectionChangedDelegate SelectionChanged;

        #endregion

        #region Properties

        #region Collection Model

        private ObservableCollection<T> _collectionModel;

        /// <summary>
        /// The collection model of this <see cref="ComboBox"/>.
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
        /// The elements of the <see cref="ComboBox"/>.
        /// </summary>
        public ObservableCollection<SelectableElementViewModel<T>> Elements { get; set; } = new ObservableCollection<SelectableElementViewModel<T>>();

        /// <summary>
        /// The one and only selected element of the <see cref="ComboBox"/>.
        /// </summary>
        public SelectableElementViewModel<T> SelectedElement { get; set; }

        /// <summary>
        /// The name of the <see cref="SelectedElement"/>.
        /// </summary>
        public string SelectedElementName => SelectedElement == null ? NoSelectedElementString : SelectedElement.Name;

        /// <summary>
        /// Indicates whether the <see cref="Elements"/> list is empty or not.
        /// </summary>
        public bool IsElementsListEmpty => Elements.Count == 0;

        /// <summary>
        /// The name which is displayed when there is no elements selected.
        /// </summary>
        private static string NoSelectedElementString => "n/a";

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
            SelectedElement = null;
            foreach (var nameable in CollectionModel)
            {
                AddElement(nameable);
            }
            if (Elements.Count > 0)
            {
                SelectElement(Elements[0]);
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
                if (SelectedElement == element)
                {
                    SelectedElement = null;
                }
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
                SelectedElement = null;
                if (e.NewItems != null)
                {
                    foreach (var element in e.NewItems)
                    {
                        AddElement(element as T);
                    }
                    if (Elements.Count > 0)
                    {
                        SelectElement(Elements[0]);
                    }
                }
            }
            OnPropertyChanged("");
        }

        /// <summary>
        /// Called whenever a <see cref="SelectableElementViewModel.SelectionChanged"/> event is fired.
        /// </summary>
        /// <param name="element">The <see cref="SelectableElementViewModel{T}"/>.</param>
        private void OnSelectionChanged(SelectableElementViewModel<T> element) => SelectElement(element);

        /// <summary>
        /// Selects the given <see cref="SelectableElementViewModel{T}"/>.
        /// </summary>
        /// <param name="element">The <see cref="SelectableElementViewModel{T}"/> to select.</param>
        private void SelectElement(SelectableElementViewModel<T> element)
        {
            if (SelectedElement != null)
            {
                SelectedElement.IsSelected = false;
            }
            SelectedElement = element;
            SelectedElement.IsSelected = true;
            SelectionChanged?.Invoke(element.Model);
        }


        /// <summary>
        /// Selects the a <see cref="SelectableElementViewModel{T}"/> by the given <see cref="T"/> model.
        /// </summary>
        /// <param name="model">The <see cref="T"/> model.</param>
        public void SelectElement(T model)
        {
            var element = Elements.FirstOrDefault(_ => _.Model == model);
            if (element != null)
            {
                SelectElement(element);
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new empty instance of the <see cref="ComboBoxViewModel{T}"/> class.
        /// </summary>
        public ComboBoxViewModel() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComboBoxViewModel{T}"/> class.
        /// </summary>
        /// <param name="collectionModel">The collection model to load.</param>
        public ComboBoxViewModel(ObservableCollection<T> collectionModel)
        {
            LoadCollectionModel(collectionModel);
        }

        #endregion
    }
}
