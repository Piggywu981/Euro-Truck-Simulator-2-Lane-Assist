from ETS2LA.Plugin import ETS2LAPlugin
import threading
import importlib
import time

class HUDWidget():
    plugin: ETS2LAPlugin
    data: list
    fps: int = 2
    
    name: str
    description: str
    
    def __init__(self, plugin: ETS2LAPlugin):
        self.plugin = plugin
        self.data = []
        
    def settings(self) -> dict:
        return {}

    def draw(self, offset_x: int, width: int, height: int = 50):
        ...
        
class HUDRenderer():
    plugin: ETS2LAPlugin
    data: list
    fps: int = 2
    
    name: str
    description: str
    
    def __init__(self, plugin: ETS2LAPlugin):
        self.plugin = plugin
        self.data = []
        
    def settings(self) -> dict:
        return {}
        
    def draw(self):
        ...
        
class ElementRunner():
    element: HUDRenderer | HUDWidget

    offset_x: int = 0
    width: int = 0
    height: int = 50
    
    enabled: bool = False
    data: list = []

    def __init__(self, plugin: ETS2LAPlugin, name: str):
        path = f"Plugins.NGHUD.elements.{name}"
        print(f"Loading NGHUD element: {path}")
        module = importlib.import_module(path)
        # Try to find "Renderer" or "Widget" class in the module
        try:
            self.element = module.Renderer(plugin)
        except:
            try:
                self.element = module.Widget(plugin)
            except AttributeError:
                raise ImportError(f"Element {name} does not have a Renderer or Widget class.")
        
        threading.Thread(target=self.run_element, daemon=True).start()
        
    def run_element(self):
        while True:
            time.sleep(1 / self.element.fps)
            
            if not self.enabled:
                continue

            if isinstance(self.element, HUDRenderer):
                self.element.draw()
            elif isinstance(self.element, HUDWidget):
                self.element.draw(self.offset_x, self.width, self.height)
            self.data = self.element.data
