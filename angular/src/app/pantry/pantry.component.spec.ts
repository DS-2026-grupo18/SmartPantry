import '@angular/compiler';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { of, throwError } from 'rxjs';
import { FormBuilder } from '@angular/forms';
import { Confirmation } from '@abp/ng.theme.shared';
import { PantryComponent } from './pantry.component';
import { PantryItemDto } from '../proxy/pantry/models';
import { Injector, runInInjectionContext } from '@angular/core';
import { ConfirmationService } from '@abp/ng.theme.shared';
import { PantryItemService } from '../proxy/pantry/pantry-item.service';
describe('PantryComponent', () => {
  let component: PantryComponent;
  let mockPantryService: any;
  let mockConfirmationService: any;

  const fakeItems: PantryItemDto[] = [
    {
      id: '1',
      userId: 'u1',
      productId: 'p1',
      productName: 'Leche',
      status: 1,
      expirationDate: '2026-11-20T00:00:00Z',
      quantity: 2,
      unit: 'Litros',
    },
  ];

  beforeEach(() => {
    mockPantryService = {
      getList: vi.fn().mockReturnValue(of({ items: fakeItems, totalCount: 1 })),
      update: vi.fn().mockReturnValue(of(fakeItems[0])),
      consume: vi.fn().mockReturnValue(of(fakeItems[0])),
      delete: vi.fn().mockReturnValue(of(undefined)),
    };

    mockConfirmationService = {
      warn: vi.fn().mockReturnValue(of(Confirmation.Status.confirm)),
    };

    const injector = Injector.create({
      providers: [
        { provide: PantryItemService, useValue: mockPantryService },
        { provide: ConfirmationService, useValue: mockConfirmationService },
        { provide: FormBuilder, useValue: new FormBuilder() },
      ],
    });

    component = runInInjectionContext(injector, () => new PantryComponent());
    component.crearFormulario();
  });

  it('debe iniciar y cargar la lista de items correctamente (estado con resultados)', () => {
    component.cargarDatos();

    expect(component.isLoading).toBe(false);
    expect(component.hasError).toBe(false);
    expect(component.items.length).toBe(1);
    expect(component.items[0].productName).toBe('Leche');
  });

  it('debe manejar el estado de lista vacia', () => {
    mockPantryService.getList.mockReturnValue(of({ items: [], totalCount: 0 }));

    component.cargarDatos();

    expect(component.isLoading).toBe(false);
    expect(component.hasError).toBe(false);
    expect(component.items.length).toBe(0);
  });

  it('debe manejar el estado de error cuando falla el backend', () => {
    mockPantryService.getList.mockReturnValue(throwError(() => new Error('Error de servidor')));

    component.cargarDatos();

    expect(component.isLoading).toBe(false);
    expect(component.hasError).toBe(true);
    expect(component.items.length).toBe(0);
  });

  it('debe validar que la cantidad no sea negativa en el formulario de edicion', () => {
    component.abrirEditar(fakeItems[0]);

    component.form.patchValue({ quantity: -5 });
    expect(component.form.invalid).toBe(true);
    expect(component.form.get('quantity')?.errors?.['min']).toBeTruthy();

    component.guardar();
    expect(mockPantryService.update).not.toHaveBeenCalled();
  });

  it('debe permitir guardar cuando los datos de edicion son validos', () => {
    component.abrirEditar(fakeItems[0]);

    component.form.patchValue({ quantity: 10, unit: 'Cajas' });
    expect(component.form.valid).toBe(true);

    component.guardar();
    expect(mockPantryService.update).toHaveBeenCalled();
  });

  it('debe eliminar el item si el usuario confirma', () => {
    mockConfirmationService.warn.mockReturnValue(of(Confirmation.Status.confirm));

    component.eliminar(fakeItems[0]);

    expect(mockPantryService.delete).toHaveBeenCalledWith('1');
  });

  it('no debe eliminar el item si el usuario cancela la confirmacion', () => {
    mockConfirmationService.warn.mockReturnValue(of(Confirmation.Status.reject));

    component.eliminar(fakeItems[0]);

    expect(mockPantryService.delete).not.toHaveBeenCalled();
  });
});
