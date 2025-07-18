import logging
import json
import os

class Settings(object):
    last_update = 0
    
    def __init__(self, path: str) -> None:
        self._path = path
        self._settings = {}
        if not os.path.exists(f"{self._path}/settings.json"):
            os.makedirs(self._path, exist_ok=True)
            self._save()
        else:
            self._load()

    def _load(self):
        try:
            self.last_update = os.path.getmtime(f"{self._path}/settings.json")
            with open(f"{self._path}/settings.json", "r") as file:
                self._settings = json.load(file)
        except:
            logging.warning("Failed to load settings, you can ignore this if it's not repeating consistently.")

    def _save(self):
        with open(f"{self._path}/settings.json", "w") as file:
            json.dump(self._settings, file, indent=4)

    def __getattr__(self, name):
        if self.last_update != os.path.getmtime(f"{self._path}/settings.json"):
            self._load()
        
        if name in self._settings:
            return self._settings[name]
        
        logging.warning(f"Setting '{name}' not found in settings file")
        return None

    def __setattr__(self, name, value):
        if name in ['_path', '_settings', 'last_update']:
            super().__setattr__(name, value)
        else:
            self._settings[name] = value
            self._save()