using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Windows.Input;
using Plugin.FilePicker.Abstractions;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages.Relatives
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditRelative : ContentPage
    {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

		public EditRelative(int id)
		{
			InitializeComponent();
			BindingContext = new ViewModel(id);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Model.OnAppearing();
		}
		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			Model.OnDisappearing();
		}

		public class ViewModel : BaseViewModel
		{
			private int Id { get; set; }

			public ViewModel(int id) : base()
			{
				Id = id;

                Pin = new CustomElements.CustomMap.Pin(id, string.Empty, string.Empty, null, default(Position));
                Pins = new List<CustomElements.CustomMap.Pin>() {
                    Pin
                };

				PersonTypeChanged += (sender, type) => this.IsDeadPerson = type == Models.PersonType.Dead;

				UserPositionChanged += (sender, position) => {
					UserPosition = position;
					Latitude = position.Latitude.ToString();
					Longitude = position.Longitude.ToString();
				};

                PersonPositionChanged += (sender, position) =>
                {
                    Pin.Position = position;
                    Coordinates = $"{position.Latitude}\n{position.Longitude}";
                };

                IsDeadPersonChanged += (sender, e) => IsSchemeVisible = Device.RuntimePlatform == Device.iOS ? false : e;

				ResetCommand.Execute(null);
			}

            public override void OnAppearing()
            {
                base.OnAppearing();
                IsMapVisible = false;
            }

			#region Person type

			private Dictionary<Models.PersonType, string> personTypeDictionary { get; } = new Dictionary<Models.PersonType, string> {
				{ Models.PersonType.Dead, "Мертвый" },
				{ Models.PersonType.Alive, "Живой" }
			};
			public List<string> PersonTypeList =>
				personTypeDictionary.Select(i => i.Value).ToList();

			private Models.PersonType Type
			{
				get => personTypeDictionary.ElementAt(PersonTypeIndex).Key;
				set => PersonTypeIndex = PersonTypeList.IndexOf(personTypeDictionary[value]);
			}

			private int _personTypeIndex;
			public int PersonTypeIndex
			{
				get => _personTypeIndex;
				set
				{
					_personTypeIndex = value;
					OnPropertyChanged(nameof(PersonTypeIndex));
					PersonTypeChanged?.Invoke(this, Type);
				}
			}

			private event EventHandler<Models.PersonType> PersonTypeChanged;

			#endregion

			#region Map

			private Position _cameraPosition;
			public Position CameraPosition
			{
				get => _cameraPosition;
				set
				{
					if (_cameraPosition != value)
					{
						_cameraPosition = value;
						OnPropertyChanged(nameof(CameraPosition));
					}
				}
			}

			private List<Libmemo.CustomElements.CustomMap.Pin> _pins;
			public List<Libmemo.CustomElements.CustomMap.Pin> Pins
			{
				get => _pins;
				set
				{
					if (_pins != value)
					{
						_pins = value;
						OnPropertyChanged(nameof(Pins));
					}
				}
			}
            private CustomElements.CustomMap.Pin Pin { get; set; }


            private string _coordinates;
            public string Coordinates {
                get => _coordinates;
                set {
                    if (_coordinates != value) {
                        _coordinates = value;
                        OnPropertyChanged(nameof(Coordinates));
                    }
                }
            }

			private Position? UserPosition { get; set; }


            private event EventHandler<Position> PersonPositionChanged;
            public ICommand MapClickCommand => new Command<Position>(position => {
                PersonPositionChanged?.Invoke(this, position);
            });

            private event EventHandler<Position> UserPositionChanged;
            public ICommand UserPositionChangedCommand => new Command<Position>(position => {
                UserPositionChanged?.Invoke(this, position);
            });



			private string _latitude = "\u2014";
			public string Latitude
			{
				get => _latitude;
				set
				{
					if (_latitude != value)
					{
						_latitude = value;
						OnPropertyChanged(nameof(Latitude));
					}
				}
			}

			private string _longitude = "\u2014";
			public string Longitude
			{
				get => _longitude;
				set
				{
					if (_longitude != value)
					{
						_longitude = value;
						OnPropertyChanged(nameof(Longitude));
					}
				}
			}


            private bool _isMapVisible = true;
			public bool IsMapVisible
			{
				get => _isMapVisible;
				set
				{
					_isMapVisible = value;
					OnPropertyChanged(nameof(IsMapVisible));
				}
			}

			public ICommand HideMap => new Command(() => IsMapVisible = false);
            public ICommand ShowMap => new Command(() => {
                IsMapVisible = true;
				if (Pin.Position != default(Position))
					CameraPosition = Pin.Position;
            });


			#endregion

            private event EventHandler<bool> IsDeadPersonChanged;
			private bool _isDeadPerson = true;
			public bool IsDeadPerson
			{
				get => _isDeadPerson;
				set
				{
					_isDeadPerson = value;
                    IsDeadPersonChanged?.Invoke(this, value);
					OnPropertyChanged(nameof(IsDeadPerson));
				}
			}


			private string _firstName;
			public string FirstName
			{
				get => _firstName;
				set
				{
					if (_firstName != value)
					{
						_firstName = value;
						this.OnPropertyChanged(nameof(FirstName));
					}
				}
			}

			private string _secondName;
			public string SecondName
			{
				get => _secondName;
				set
				{
					if (_secondName != value)
					{
						_secondName = value;
						this.OnPropertyChanged(nameof(SecondName));
					}
				}
			}

			private string _lastName;
			public string LastName
			{
				get => _lastName;
				set
				{
					if (_lastName != value)
					{
						_lastName = value;
						this.OnPropertyChanged(nameof(LastName));
					}
				}
			}

			private DateTime? _dateBirth = null;
			public DateTime? DateBirth
			{
				get => _dateBirth;
				set
				{
					if (_dateBirth != value)
					{
						_dateBirth = value;
						this.OnPropertyChanged(nameof(DateBirth));
					}
				}
			}

			private DateTime? _dateDeath = null;
			public DateTime? DateDeath
			{
				get => _dateDeath;
				set
				{
					if (_dateDeath != value)
					{
						_dateDeath = value;
						this.OnPropertyChanged(nameof(DateDeath));
					}
				}
			}

			private string _text;
			public string Text
			{
				get => _text;
				set
				{
					if (_text != value)
					{
						_text = value;
						this.OnPropertyChanged(nameof(Text));
					}
				}
			}



			private List<(ImageSource, int?)> _photos = new List<(ImageSource, int?)>();
			private List<(ImageSource, int?)> Photos
			{
				get => _photos;
				set
				{
					if (_photos != value)
					{
						_photos = value;
						OnPropertyChanged(nameof(ImageCollection));
					}
				}
			}
			public class Image
			{
				public ImageSource PhotoSource { get; set; }
				public ICommand PickPhotoCommand { get; set; }
				public ICommand MakePhotoCommand { get; set; }
				public bool IsDeleteAviliable { get; set; }
				public ICommand DeletePhotoCommand { get; set; }
			}
			public List<Image> ImageCollection => Photos.Select(i => new Image
			{
				PhotoSource = i.Item1,
				PickPhotoCommand = new Command(async () => {
					if (CrossMedia.Current.IsPickPhotoSupported)
					{
						var photo = await CrossMedia.Current.PickPhotoAsync();
						if (photo == null) return;

						var index = Photos.FindIndex(j => j.Item1 == i.Item1);
						var item = Photos[index];
						item.Item1 = ImageSource.FromFile(photo.Path);
						Photos[index] = item;

						OnPropertyChanged(nameof(ImageCollection));
					}
					else
					{
						App.ToastNotificator.Show("Выбор фото невозможен");
					}
				}),
				MakePhotoCommand = new Command(async () => {
					if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported)
					{
						var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { SaveToAlbum = false, PhotoSize = PhotoSize.Medium });
						if (file == null) return;

						var index = Photos.FindIndex(j => j.Item1 == i.Item1);
						var item = Photos[index];
						item.Item1 = ImageSource.FromFile(file.Path);
						Photos[index] = item;

						OnPropertyChanged(nameof(ImageCollection));
					}
					else
					{
						App.ToastNotificator.Show("Сделать фото невозможно");
					}
				}),
				IsDeleteAviliable = true,
				DeletePhotoCommand = new Command(() => {
					var index = Photos.FindIndex(j => j.Item1 == i.Item1);
					Photos.RemoveAt(index);

					OnPropertyChanged(nameof(ImageCollection));
				})
			}).Concat(new List<Image> {
				new Image {
					PhotoSource = ImageSource.FromFile("placeholder"),
					PickPhotoCommand = new Command(async () => {
						if (CrossMedia.Current.IsPickPhotoSupported) {
							var photo = await CrossMedia.Current.PickPhotoAsync();
							if (photo == null) return;

							(ImageSource, int?) item = (ImageSource.FromFile(photo.Path), null);
							Photos.Add(item);

							OnPropertyChanged(nameof(ImageCollection));
						} else {
							App.ToastNotificator.Show("Сделать фото невозможно");
						}
					}),
					MakePhotoCommand = new Command(async () => {
						if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported) {
							var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { SaveToAlbum = false, PhotoSize = PhotoSize.Medium });
							if (file == null) return;

							(ImageSource, int?) item = (ImageSource.FromFile(file.Path), null);
							Photos.Add(item);

							OnPropertyChanged(nameof(ImageCollection));
						} else {
							App.ToastNotificator.Show("Сделать фото невозможно");
						}
					}),
					IsDeleteAviliable = false
				}
			}).ToList();




			private double? _height;
			public double? Height
			{
				get => _height;
				set
				{
					if (_height != value)
					{
						_height = value;
						this.OnPropertyChanged(nameof(Height));
					}
				}
			}
			private double? _width;
			public double? Width
			{
				get => _width;
				set
				{
					if (_width != value)
					{
						_width = value;
						this.OnPropertyChanged(nameof(Width));
					}
				}
			}

			private byte[] SchemeArray { get; set; }
			private string _schemeName;
			public string SchemeName
			{
				get => string.IsNullOrWhiteSpace(_schemeName) ? "Не выбрано" : _schemeName;
				private set
				{
					if (_schemeName != value)
					{
						_schemeName = value;
						OnPropertyChanged(nameof(SchemeName));
					}
				}
			}
			private void SetScheme(string name, byte[] stream)
			{
				ResetScheme();

				SchemeName = name;
				SchemeArray = stream;
			}
			private void ResetScheme()
			{
				SchemeName = null;
				SchemeArray = null;
			}
			public ICommand SelectSchemeCommand => new Command(async () => {
				FileData file;
				try
				{
					file = await Plugin.FilePicker.CrossFilePicker.Current.PickFile();
				}
				catch
				{
					App.ToastNotificator.Show("Не удалось выбрать файл");
					return;
				}

				if (file == null || file.DataArray.Length == 0) return;

				var fileName = !string.IsNullOrWhiteSpace(file.FileName)
									  ? file.FileName
									  : "Файл";

				SetScheme(fileName, file.DataArray);
			});
			private bool _isSchemeVisible;
			public bool IsSchemeVisible
			{
				get => _isSchemeVisible;
				set
				{
					_isSchemeVisible = value;
					OnPropertyChanged(nameof(IsSchemeVisible));
				}
			}


			private Uri _schemeUrl = null;
			protected Uri SchemeUrl
			{
				get => _schemeUrl;
				set
				{
					if (this._schemeUrl != value)
					{
						this._schemeUrl = value;
						this.OnPropertyChanged(nameof(IsSchemeCanDownload));
					}
				}
			}
			public bool IsSchemeCanDownload => SchemeUrl != null;
			public ICommand SchemeDownloadCommand => new Command(() => {
				if (SchemeUrl != null) Device.OpenUri(SchemeUrl);
			});


			private string _section;
			public string Section
			{
				get => _section;
				set
				{
					if (_section != value)
					{
						_section = value;
						OnPropertyChanged(nameof(Section));
					}
				}
			}

			private double? _graveNumber;
			public double? GraveNumber
			{
				get => _graveNumber;
				set
				{
					if (_graveNumber != value)
					{
						_graveNumber = value;
						OnPropertyChanged(nameof(GraveNumber));
					}
				}
			}


			private void SetData(Models.Person person)
			{
				this.Type = person is Models.DeadPerson
					? Models.PersonType.Dead
					: Models.PersonType.Alive;

				this.FirstName = person.FirstName;
				this.LastName = person.LastName;
				this.SecondName = person.SecondName;
				if (person.DateBirth.HasValue) this.DateBirth = person.DateBirth.Value;

				Photos = person.Images.Select(i => (ImageSource.FromUri(i.Value), (int?)i.Key)).ToList();


				if (person is Models.DeadPerson)
				{
					var deadPerson = (Models.DeadPerson)person;

                    this.Pin.Position = new Position(deadPerson.Latitude, deadPerson.Longitude);
                    this.Coordinates = $"{deadPerson.Latitude.ToString()}\n{deadPerson.Longitude.ToString()}";
					if (deadPerson.DateDeath.HasValue) this.DateDeath = deadPerson.DateDeath.Value;
					this.Height = deadPerson.Height;
					this.Width = deadPerson.Width;
					this.Text = deadPerson.Text;

					if (deadPerson.Scheme != null)
					{
						SchemeUrl = deadPerson.Scheme;
					}

					this.Section = deadPerson.Section;
					this.GraveNumber = deadPerson.GraveNumber;



                    CameraPosition = this.Pin.Position;
				}
			}

			public ICommand ResetCommand => new Command(async () => {
				if (cancelTokenSource != null) return;

				StartLoading("Получение данных");

				HttpResponseMessage response;
				try
				{
					cancelTokenSource = new CancellationTokenSource();
					response = await WebClient.Instance.SendAsync(HttpMethod.Get, new Uri($"{Settings.RELATIVES_URL}{Id}/"), null, 30, cancelTokenSource.Token);
				}
				catch (TimeoutException)
				{
					App.ToastNotificator.Show("Превышен интервал запроса");
					return;
				}
				catch (OperationCanceledException)
				{ //cancel
					return;
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка запроса");
					return;
				}
				finally
				{
					cancelTokenSource = null;
					StopLoading();
				}

				var str = await response.Content.ReadAsStringAsync();

				try
				{
					if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					{
						throw new UnauthorizedAccessException();
					}
					if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
					{
						var msg = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Error>(str);
						throw new HttpRequestException(msg.error);
					}
					response.EnsureSuccessStatusCode();
				}
				catch (UnauthorizedAccessException)
				{
					await AuthHelper.ReloginAsync();
					return;
				}
				catch (HttpRequestException ex)
				{
					App.ToastNotificator.Show(ex.Message);
					return;
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка");
					return;
				}


				Models.Person person;
				try
				{
					var json = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Person>(str);
					person = Models.Person.Convert(json);
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка ответа сервера");
					return;
				}

				SetData(person);
			});

			public ICommand SaveCommand => new Command(async () => {
				if (cancelTokenSource != null) return;

                if (this.IsDeadPerson && this.Pin.Position == default(Position))
				{
					App.ToastNotificator.Show("Не указано местоположение");
					return;
				}
				if (string.IsNullOrWhiteSpace(this.FirstName))
				{
					App.ToastNotificator.Show("Поле \"Имя\" не заполнено");
					return;
				}

				StartLoading("Сохранение");

				var content = new MultipartFormDataContent(String.Format("----------{0:N}", Guid.NewGuid()));
				content.Add(new StringContent(IsDeadPerson ? "dead" : "alive"), "type");
				content.Add(new StringContent(this.FirstName), "first_name");
				if (this.SecondName != null)
					content.Add(new StringContent(this.SecondName), "second_name");
				if (this.LastName != null)
					content.Add(new StringContent(this.LastName), "last_name");
				if (this.DateBirth.HasValue)
					content.Add(new StringContent(this.DateBirth.Value.ToString("yyyy-MM-dd")), "date_birth");


				foreach (var photo in Photos)
				{
					if (photo.Item1 is FileImageSource && !photo.Item2.HasValue)
					{
						var result = DependencyService.Get<IFileStreamPicker>().GetFile((photo.Item1 as FileImageSource).File);
                        content.Add(new StreamContent(result.Stream), "new_photos[]", $"photo{Path.GetExtension(result.Name)}");
					}
					else if (photo.Item1 is FileImageSource && photo.Item2.HasValue)
					{
						var result = DependencyService.Get<IFileStreamPicker>().GetFile((photo.Item1 as FileImageSource).File);
                        content.Add(new StreamContent(result.Stream), $"update_photos[{photo.Item2.Value}]", $"photo{Path.GetExtension(result.Name)}");
					}
					else if (photo.Item1 is UriImageSource && photo.Item2.HasValue)
					{
						content.Add((new StringContent(photo.Item2.Value.ToString())), "keep_photos[]");
					}

				}

				if (this.IsDeadPerson)
				{
                    content.Add(new StringContent(this.Pin.Position.Latitude.ToString(CultureInfo.InvariantCulture)), "latitude");
                    content.Add(new StringContent(this.Pin.Position.Longitude.ToString(CultureInfo.InvariantCulture)), "longitude");

					if (this.DateDeath.HasValue)
						content.Add(new StringContent(this.DateDeath.Value.ToString("yyyy-MM-dd")), "date_death");
					if (this.Text != null)
						content.Add(new StringContent(this.Text), "text");
					if (this.Height.HasValue)
						content.Add(new StringContent(this.Height.Value.ToString(CultureInfo.InvariantCulture)), "height");
					if (this.Width.HasValue)
						content.Add(new StringContent(this.Width.Value.ToString(CultureInfo.InvariantCulture)), "width");
					if (this.SchemeArray != null)
					{
                        content.Add(new ByteArrayContent(this.SchemeArray), "scheme", $"file{Path.GetExtension(SchemeName)}");
					}
					if (!string.IsNullOrWhiteSpace(this.Section))
					{
						content.Add(new StringContent(this.Section), "section");
					}
					if (this.GraveNumber.HasValue)
					{
						content.Add(new StringContent(this.GraveNumber.Value.ToString(CultureInfo.InvariantCulture)), "grave_number");
					}
				}



				HttpResponseMessage response;
				try
				{
					cancelTokenSource = new CancellationTokenSource();
					response = await WebClient.Instance.SendAsync(HttpMethod.Post, new Uri($"{Settings.RELATIVES_URL}{Id}/"), content, 60, cancelTokenSource.Token);
				}
				catch (TimeoutException)
				{
					App.ToastNotificator.Show("Превышен интервал запроса");
					return;
				}
				catch (OperationCanceledException)
				{ //cancel
					return;
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка");
					return;
				}
				finally
				{
					cancelTokenSource = null;
					StopLoading();
				}

				var str = await response.Content.ReadAsStringAsync();

				try
				{
					if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					{
						throw new UnauthorizedAccessException();
					}
					if (response.StatusCode == System.Net.HttpStatusCode.BadRequest
						|| response.StatusCode == System.Net.HttpStatusCode.NotFound
						|| response.StatusCode == System.Net.HttpStatusCode.Forbidden
						|| response.StatusCode == System.Net.HttpStatusCode.InternalServerError
					)
					{
						var msg = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Error>(str);
						throw new HttpRequestException(msg.error);
					}
					response.EnsureSuccessStatusCode();
				}
				catch (UnauthorizedAccessException)
				{
					await AuthHelper.ReloginAsync();
					return;
				}
				catch (HttpRequestException ex)
				{
					App.ToastNotificator.Show(ex.Message);
					return;
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка");
					return;
				}


				Models.Person person;
				try
				{
					var json = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Person>(str);
					person = Models.Person.Convert(json);
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка ответа сервера");
					return;
				}


				SetData(person);
				App.ToastNotificator.Show("Сохранено");
			});

			public ICommand DeleteCommand => new Command(async () => {
				if (cancelTokenSource != null) return;

				if (!await App.Current.MainPage.DisplayAlert("Удаление", "Вы уверены?", "Да", "Нет")) return;

				StartLoading("Удаление");

				HttpResponseMessage response;
				try
				{
					cancelTokenSource = new CancellationTokenSource();
					response = await WebClient.Instance.SendAsync(HttpMethod.Delete, new Uri($"{Settings.RELATIVES_URL}{Id}/"), null, 30, cancelTokenSource.Token);
				}
				catch (TimeoutException)
				{
					App.ToastNotificator.Show("Превышен интервал запроса");
					return;
				}
				catch (OperationCanceledException)
				{ //cancel
					return;
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка запроса");
					return;
				}
				finally
				{
					cancelTokenSource = null;
					StopLoading();
				}

				var str = await response.Content.ReadAsStringAsync();

				try
				{
					if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					{
						throw new UnauthorizedAccessException();
					}
					if (response.StatusCode == System.Net.HttpStatusCode.NotFound
						|| response.StatusCode == System.Net.HttpStatusCode.Forbidden
						|| response.StatusCode == System.Net.HttpStatusCode.InternalServerError
					)
					{
						var msg = Newtonsoft.Json.JsonConvert.DeserializeObject<Json.Error>(str);
						throw new HttpRequestException(msg.error);
					}
					response.EnsureSuccessStatusCode();
				}
				catch (UnauthorizedAccessException)
				{
					await AuthHelper.ReloginAsync();
					return;
				}
				catch (HttpRequestException ex)
				{
					App.ToastNotificator.Show(ex.Message);
					return;
				}
				catch
				{
					App.ToastNotificator.Show("Ошибка");
					return;
				}


				App.ToastNotificator.Show("Удалено");
				await App.GlobalPage.Pop();
			});
		}
    }
}
